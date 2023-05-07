// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.CodeAnalysis.Operations;
using KeyType = System.Tuple<Microsoft.CodeAnalysis.Compilation, string, string>;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

internal sealed class CompleteOnNormalPathAnalysisHelper
{
	private static readonly BoundedCacheWithFactory<KeyType, CompleteOnNormalPathAnalysisHelper> HelperCache = new();

	private static readonly ImmutableHashSet<OperationKind> TargetCreationKinds = ImmutableHashSet.Create(
		OperationKind.ObjectCreation,
		OperationKind.TypeParameterObjectCreation,
		OperationKind.DynamicObjectCreation,
		OperationKind.Invocation);

	private readonly WellKnownTypeProvider _wellKnownTypeProvider;

	public INamedTypeSymbol? TargetType { get; }

	public IMethodSymbol? CompleteMethod { get; }

	private CompleteOnNormalPathAnalysisHelper(Compilation compilation, string targetTypeFullName, string completeMethodName)
	{
		_wellKnownTypeProvider = WellKnownTypeProvider.GetOrCreate(compilation);
		TargetType = _wellKnownTypeProvider.GetOrCreateTypeByMetadataName(targetTypeFullName);
		CompleteMethod = TargetType?.GetMembers(completeMethodName).Where(m => m.Kind == SymbolKind.Method && !m.IsStatic).OfType<IMethodSymbol>().FirstOrDefault();
	}

	public static bool TryGetOrCreate(
		Compilation compilation,
		string targetTypeFullName,
		string completeMethodName,
		[NotNullWhen(returnValue: true)] out CompleteOnNormalPathAnalysisHelper? helper
	)
	{
		helper = HelperCache.GetOrCreateValue(Tuple.Create(compilation, targetTypeFullName, completeMethodName), CreateCompleteOnNormalPathAnalysisHelper);
		if (helper.TargetType == null || helper.CompleteMethod == null)
		{
			helper = null;
			return false;
		}

		return true;

		// Local functions
		static CompleteOnNormalPathAnalysisHelper CreateCompleteOnNormalPathAnalysisHelper(KeyType key)
			=> new(key.Item1, key.Item2, key.Item3);
	}

	public bool TryGetOrComputeResult(
		ImmutableArray<IOperation> operationBlocks,
		IMethodSymbol containingMethod,
		AnalyzerOptions analyzerOptions,
		DiagnosticDescriptor rule,
		PointsToAnalysisKind defaultPointsToAnalysisKind,
		string targetTypeFullName,
		string completeMethodName,
		bool trackInstanceFields,
		[NotNullWhen(returnValue: true)] out CompleteOnNormalPathAnalysisResult? completeOnNormalPathAnalysisResult,
		[NotNullWhen(returnValue: true)] out PointsToAnalysisResult? pointsToAnalysisResult,
		InterproceduralAnalysisPredicate? interproceduralAnalysisPredicate = null,
		bool defaultCompletionOwnershipTransferAtConstructor = false)
	{
		var cfg = operationBlocks.GetControlFlowGraph();
		if (cfg != null && TargetType != null && CompleteMethod != null)
		{
			completeOnNormalPathAnalysisResult =
				CompleteOnNormalPathAnalysis.TryGetOrComputeResult(
					cfg, containingMethod, _wellKnownTypeProvider, analyzerOptions, rule, defaultPointsToAnalysisKind,
					targetTypeFullName, completeMethodName, trackInstanceFields,
					out pointsToAnalysisResult, interproceduralAnalysisPredicate: interproceduralAnalysisPredicate,
					defaultCompletionOwnershipTransferAtConstructor: defaultCompletionOwnershipTransferAtConstructor);
			if (completeOnNormalPathAnalysisResult != null)
			{
				RoslynDebug.Assert(pointsToAnalysisResult is object);
				return true;
			}
		}

		completeOnNormalPathAnalysisResult = null;
		pointsToAnalysisResult = null;
		return false;
	}

	private bool HasCompletionOwnershipTransferForConstructorParameter(IMethodSymbol containingMethod)
		=> containingMethod.MethodKind == MethodKind.Constructor;

	private bool IsTargetCreation(IOperation operation)
		=> (TargetCreationKinds.Contains(operation.Kind) ||
			operation.Parent is IArgumentOperation argument && argument.Parameter.RefKind == RefKind.Out) &&
		   IsTarget(operation.Type, TargetType!);

	public bool HasAnyTargetCreationDescendant(ImmutableArray<IOperation> operationBlocks, IMethodSymbol containingMethod)
	{
		return operationBlocks.HasAnyOperationDescendant(IsTargetCreation) ||
			HasCompletionOwnershipTransferForConstructorParameter(containingMethod);
	}

	//public bool IsDisposableTypeNotRequiringToBeDisposed(ITypeSymbol typeSymbol) =>
	//	// Common case doesn't require dispose. https://learn.microsoft.com/dotnet/api/system.threading.tasks.task.dispose
	//	typeSymbol.DerivesFrom(Task, baseTypesOnly: true) ||
	//	// StringReader doesn't need to be disposed: https://learn.microsoft.com/dotnet/api/system.io.stringreader
	//	SymbolEqualityComparer.Default.Equals(typeSymbol, StringReader) ||
	//	// MemoryStream doesn't need to be disposed. https://learn.microsoft.com/dotnet/api/system.io.memorystream
	//	// Subclasses *might* need to be disposed, but that is the less common case,
	//	// and the common case is a huge source of noisy warnings.
	//	SymbolEqualityComparer.Default.Equals(typeSymbol, MemoryStream);

	//public ImmutableHashSet<IFieldSymbol> GetDisposableFields(INamedTypeSymbol namedType)
	//{
	//	EnsureDisposableFieldsMap();
	//	RoslynDebug.Assert(_lazyDisposableFieldsMap != null);

	//	if (_lazyDisposableFieldsMap.TryGetValue(namedType, out ImmutableHashSet<IFieldSymbol> disposableFields))
	//	{
	//		return disposableFields;
	//	}

	//	if (!namedType.IsDisposable(IDisposable, IAsyncDisposable))
	//	{
	//		disposableFields = ImmutableHashSet<IFieldSymbol>.Empty;
	//	}
	//	else
	//	{
	//		disposableFields = namedType.GetMembers()
	//			.OfType<IFieldSymbol>()
	//			.Where(f => IsDisposable(f.Type) && !IsDisposableTypeNotRequiringToBeDisposed(f.Type))
	//			.ToImmutableHashSet();
	//	}

	//	return _lazyDisposableFieldsMap.GetOrAdd(namedType, disposableFields);
	//}

	/// <summary>
	/// Returns true if the given <paramref name="location"/> was created for an allocation in the <paramref name="containingMethod"/>
	/// or represents a location created for a constructor parameter whose type indicates dispose ownership transfer.
	/// </summary>
	//public bool IsDisposableCreationOrDisposeOwnershipTransfer(AbstractLocation location, IMethodSymbol containingMethod)
	//{
	//	if (location.Creation == null)
	//	{
	//		return location.Symbol?.Kind == SymbolKind.Parameter &&
	//			HasDisposableOwnershipTransferForConstructorParameter(containingMethod);
	//	}

	//	return IsDisposableCreation(location.Creation);
	//}

	//public bool IsDisposable([NotNullWhen(returnValue: true)] ITypeSymbol? type)
	//	=> type != null && type.IsDisposable(IDisposable, IAsyncDisposable);

	//public DisposeMethodKind GetDisposeMethodKind(IMethodSymbol method)
	//	=> method.GetDisposeMethodKind(IDisposable, IAsyncDisposable, Task, ValueTask);

	public static bool IsTarget([NotNullWhen(returnValue: true)] ITypeSymbol? type, INamedTypeSymbol targetType)
	{
		if (type is null)
		{
			return false;
		}

		// TODO: ValueType support?

		if (SymbolEqualityComparer.Default.Equals(type, targetType))
		{
			return true;
		}

		if (type.AllInterfaces.Contains(targetType))
		{
			return true;
		}

		var currentType = type.BaseType;
		while (currentType != null)
		{
			if (SymbolEqualityComparer.Default.Equals(currentType, targetType))
			{
				return true;
			}

			currentType = currentType.BaseType;
		}

		return false;
	}
}
