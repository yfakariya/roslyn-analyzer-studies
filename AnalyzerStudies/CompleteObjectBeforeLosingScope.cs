// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Analyzer.Utilities;
using Analyzer.Utilities.Extensions;
using Analyzer.Utilities.PooledObjects;
using AnalyzerStudies.CompletionOnNormalPathAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using static AnalyzerStudies.Resources;

namespace AnalyzerStudies;

// This analyzer and its dependencies are based on DisposeObjectsBeforeLosingScope and DisposeAnalysis in Roslyn Analyzers (MIT License).
// Source : https://github.com/dotnet/roslyn-analyzers/blob/v3.3.4/src/NetAnalyzers/Core/Microsoft.NetCore.Analyzers/Runtime/DisposeObjectsBeforeLosingScope.cs
//          https://github.com/dotnet/roslyn-analyzers/tree/v3.3.4/src/Utilities/FlowAnalysis/FlowAnalysis/Analysis/DisposeAnalysis
// License: https://github.com/dotnet/roslyn-analyzers/blob/v3.3.4/License.txt

/// <summary>
/// XXnnnn: Complete object before losing scope on normal path (not consider exceptional path).
/// For example, missing <c>Complete()</c> call on <c>System.Transactions.TransactionScope</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public abstract class CompleteObjectBeforeLosingScope : DiagnosticAnalyzer
{
	private readonly LocalizableString _localizableTitle;
	private readonly LocalizableString _localizableDescription;

	internal DiagnosticDescriptor NotCompletedRule { get; }

	[Obsolete] // FIXME: May be unusable
	internal DiagnosticDescriptor MayBeCompletedRule { get; }

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

	public string TargetTypeFullName { get; }

	public string CompleteMethodName { get; }

	protected CompleteObjectBeforeLosingScope(string ruleId, string targetTypeFullName, string completeMethodName)
	{
		TargetTypeFullName = targetTypeFullName;
		CompleteMethodName = completeMethodName;

		_localizableTitle = CreateLocalizableResourceString(nameof(CompleteObjectsBeforeLosingScopeTitle), targetTypeFullName, completeMethodName);
		_localizableDescription = CreateLocalizableResourceString(nameof(CompleteObjectsBeforeLosingScopeDescription), targetTypeFullName, completeMethodName);

		NotCompletedRule =
			DiagnosticDescriptorHelper.Create(
				ruleId,
				_localizableTitle,
				CreateLocalizableResourceString(nameof(CompleteObjectsBeforeLosingScopeNotCompletedMessage), targetTypeFullName, completeMethodName),
				DiagnosticCategory.Reliability,
				RuleLevel.Disabled,
				description: _localizableDescription,
				isPortedFxCopRule: true,
				isDataflowRule: true
			);

		MayBeCompletedRule =
			DiagnosticDescriptorHelper.Create(
				ruleId,
				_localizableTitle,
				CreateLocalizableResourceString(nameof(CompleteObjectsBeforeLosingScopeNotCompletedMessage), targetTypeFullName, completeMethodName),
				DiagnosticCategory.Reliability,
				RuleLevel.Disabled,
				description: _localizableDescription,
				isPortedFxCopRule: true,
				isDataflowRule: true
			);

		SupportedDiagnostics = ImmutableArray.Create(NotCompletedRule, MayBeCompletedRule);
	}

	private static LocalizableResourceString CreateLocalizableResourceString(string nameOfLocalizableResource)
		=> new(nameOfLocalizableResource, ResourceManager, typeof(Resources));

	private static LocalizableResourceString CreateLocalizableResourceString(string nameOfLocalizableResource, params string[] formatArguments)
		=> new(nameOfLocalizableResource, ResourceManager, typeof(Resources), formatArguments);

	public override void Initialize(AnalysisContext context)
	{
		context.EnableConcurrentExecution();
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.RegisterCompilationStartAction(compilationContext =>
		{
			if (!CompleteOnNormalPathAnalysisHelper.TryGetOrCreate(compilationContext.Compilation, TargetTypeFullName, CompleteMethodName, out var completeOnNormalPathAnalysisHelper))
			{
				return;
			}

			var reportedLocations = new ConcurrentDictionary<Location, bool>();
			compilationContext.RegisterOperationBlockAction(operationBlockContext =>
			{
				if (operationBlockContext.OwningSymbol is not IMethodSymbol containingMethod ||
					!completeOnNormalPathAnalysisHelper.HasAnyTargetCreationDescendant(operationBlockContext.OperationBlocks, containingMethod) ||
					operationBlockContext.Options.IsConfiguredToSkipAnalysis(NotCompletedRule, containingMethod, operationBlockContext.Compilation))
				{
					return;
				}

				var completionAnalysisKind = operationBlockContext.Options.GetCompleteOnNormalPathAnalysisKindOption(NotCompletedRule, containingMethod,
					operationBlockContext.Compilation, CompletionAnalysisKind.NonExceptionPaths);

				// For non-exception paths analysis, we can skip interprocedural analysis for certain invocations.
				var interproceduralAnalysisPredicate =
					new InterproceduralAnalysisPredicate(
						skipAnalysisForInvokedMethodPredicate: SkipInterproceduralAnalysis,
						skipAnalysisForInvokedLambdaOrLocalFunctionPredicate: null,
						skipAnalysisForInvokedContextPredicate: null);

				if (completeOnNormalPathAnalysisHelper.TryGetOrComputeResult(operationBlockContext.OperationBlocks, containingMethod,
					operationBlockContext.Options, NotCompletedRule, PointsToAnalysisKind.PartialWithoutTrackingFieldsAndProperties,
					TargetTypeFullName, CompleteMethodName, trackInstanceFields: false,
					completeOnNormalPathAnalysisResult: out var completeOnNormalPathAnalysisResult, pointsToAnalysisResult: out var pointsToAnalysisResult, interproceduralAnalysisPredicate: interproceduralAnalysisPredicate))
				{
					using var notCompletedDiagnostics = ArrayBuilder<Diagnostic>.GetInstance();
					using var mayBeNotCompletedDiagnostics = ArrayBuilder<Diagnostic>.GetInstance();

					// Compute diagnostics for undisposed objects at exit block for non-exceptional exit paths.
					var exitBlock = completeOnNormalPathAnalysisResult.ControlFlowGraph.GetExit();
					var completeDataAtExit = completeOnNormalPathAnalysisResult.ExitBlockOutput.Data;
					ComputeDiagnostics(completeDataAtExit,
						notCompletedDiagnostics, mayBeNotCompletedDiagnostics, completeOnNormalPathAnalysisResult, pointsToAnalysisResult,
						completionAnalysisKind);

					if (!notCompletedDiagnostics.Any() && !mayBeNotCompletedDiagnostics.Any())
					{
						return;
					}

					// Report diagnostics preferring *not* disposed diagnostics over may be not disposed diagnostics
					// and avoiding duplicates.
					foreach (var diagnostic in notCompletedDiagnostics.Concat(mayBeNotCompletedDiagnostics))
					{
						if (reportedLocations.TryAdd(diagnostic.Location, true))
						{
							operationBlockContext.ReportDiagnostic(diagnostic);
						}
					}
				}
			});

			return;

			// Local functions.
			bool SkipInterproceduralAnalysis(IMethodSymbol invokedMethod)
			{
				// Skip interprocedural analysis if we are invoking a method and not passing any target object as an argument
				// and not receiving a target object as a return value.
				// We also check that we are not passing any object type argument which might hold target object
				// and also check that we are not passing delegate type argument which can
				// be a lambda or local function that has access to target object in current method's scope.

				if (CanBeCompleted(invokedMethod.ReturnType))
				{
					return false;
				}

				foreach (var p in invokedMethod.Parameters)
				{
					if (CanBeCompleted(p.Type))
					{
						return false;
					}
				}

				return true;

				// Nested local functions.
				bool CanBeCompleted(ITypeSymbol type)
					=> type.SpecialType == SpecialType.System_Object ||
						type.DerivesFrom(completeOnNormalPathAnalysisHelper!.TargetType) ||
						type.TypeKind == TypeKind.Delegate;
			}
		});
	}

	private void ComputeDiagnostics(
		ImmutableDictionary<AbstractLocation, CompleteOnNormalPathAbstractValue> completeOnNormalPathData,
		ArrayBuilder<Diagnostic> notCompletedDiagnostics,
		ArrayBuilder<Diagnostic> mayBeNotCompletedDiagnostics,
		CompleteOnNormalPathAnalysisResult completeOnNormalPathAnalysisResult,
		PointsToAnalysisResult pointsToAnalysisResult,
		CompletionAnalysisKind completeAnalysisKind)
	{
		foreach (var kvp in completeOnNormalPathData)
		{
			AbstractLocation location = kvp.Key;
			CompleteOnNormalPathAbstractValue completeValue = kvp.Value;
			if (completeValue.Kind == CompleteOnNormalPathAbstractValueKind.NonTarget ||
				location.Creation == null)
			{
				continue;
			}

			var isNotCompleted = completeValue.Kind == CompleteOnNormalPathAbstractValueKind.NotCompleted ||
				(!completeValue.CompletingOrEscapingOperations.IsEmpty &&
				 completeValue.CompletingOrEscapingOperations.All(d => d.IsInsideCatchRegion(completeOnNormalPathAnalysisResult.ControlFlowGraph) && !location.GetTopOfCreationCallStackOrCreation().IsInsideCatchRegion(completeOnNormalPathAnalysisResult.ControlFlowGraph)));
			var isMayBeNotCompleted = !isNotCompleted && (completeValue.Kind == CompleteOnNormalPathAbstractValueKind.MaybeCompleted || completeValue.Kind == CompleteOnNormalPathAbstractValueKind.NotCompletedOrEscaped);

			if (isNotCompleted ||
				(isMayBeNotCompleted && completeAnalysisKind.AreMayBeNotCompletedViolationsEnabled()))
			{
				var syntax = location.TryGetNodeToReportDiagnostic(pointsToAnalysisResult);
				if (syntax == null)
				{
					continue;
				}

				var rule = GetRule(isNotCompleted);

				// Ensure that we do not include multiple lines for the object creation expression in the diagnostic message.
				var argument = syntax.ToString();
				var indexOfNewLine = argument.IndexOf(Environment.NewLine, StringComparison.Ordinal);
				if (indexOfNewLine > 0)
				{
					argument = argument[..indexOfNewLine];
				}

				var diagnostic = syntax.CreateDiagnostic(rule, argument);
				if (isNotCompleted)
				{
					notCompletedDiagnostics.Add(diagnostic);
				}
				else
				{
					mayBeNotCompletedDiagnostics.Add(diagnostic);
				}
			}
		}

		// Local functions.
		DiagnosticDescriptor GetRule(bool isNotCompleted)
			=> isNotCompleted ? NotCompletedRule : MayBeCompletedRule;
	}
}
