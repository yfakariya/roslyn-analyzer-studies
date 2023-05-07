// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using System.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using CompleteOnNormalPathAnalysisData =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DictionaryAnalysisData<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.AbstractLocation,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>;
using CompleteOnNormalPathAnalysisDomain =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.MapAbstractDomain<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.AbstractLocation,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

public sealed partial class CompleteOnNormalPathAnalysis :
	DataFlowAnalysis<
		CompleteOnNormalPathAnalysisData,
		CompleteOnNormalPathAnalysisContext,
		CompleteOnNormalPathAnalysisResult,
		CompleteOnNormalPathBlockAnalysisResult,
		CompleteOnNormalPathAbstractValue>
{
	// Invoking an instance method may likely invalidate all the instance field analysis state, i.e.
	// reference type fields might be re-assigned to point to different objects in the called method.
	// An optimistic points to analysis assumes that the points to values of instance fields don't change on invoking an instance method.
	// A pessimistic points to analysis resets all the instance state and assumes the instance field might point to any object, hence has unknown state.
	// For completion analysis, we want to perform an optimistic points to analysis as we assume a target field is not likely to be re-assigned to a separate object in helper method invocations in completion.
	private const bool PessimisticAnalysis = false;

	internal static readonly CompleteOnNormalPathAnalysisDomain CompleteOnNormalPathAnalysisDomainInstance =
		new(CompleteOnNormalPathAbstractValueDomain.Default);

	private CompleteOnNormalPathAnalysis(
		AbstractAnalysisDomain<CompleteOnNormalPathAnalysisData> analysisDomain,
		CompleteOnNormalPathDataFlowOperationVisitor operationVisitor
	) : base(analysisDomain, operationVisitor)
	{
	}

	public static CompleteOnNormalPathAnalysisResult? TryGetOrComputeResult(
		ControlFlowGraph cfg,
		ISymbol owningSymbol,
		WellKnownTypeProvider wellKnownTypeProvider,
		AnalyzerOptions analyzerOptions,
		DiagnosticDescriptor rule,
		PointsToAnalysisKind defaultPointsToAnalysisKind,
		string targetTypeFullName,
		string completeMethodName,
		bool trackInstanceFields,
		out PointsToAnalysisResult? pointsToAnalysisResult,
		InterproceduralAnalysisKind interproceduralAnalysisKind = InterproceduralAnalysisKind.ContextSensitive,
		bool performCopyAnalysisIfNotUserConfigured = false,
		InterproceduralAnalysisPredicate? interproceduralAnalysisPredicate = null,
		bool defaultCompletionOwnershipTransferAtConstructor = false,
		bool defaultCompletionOwnershipTransferAtMethodCall = false)
	{
		if (cfg == null)
		{
			throw new ArgumentNullException(nameof(cfg));
		}

		Debug.Assert(!analyzerOptions.IsConfiguredToSkipAnalysis(rule, owningSymbol, wellKnownTypeProvider.Compilation));

		var interproceduralAnalysisConfig = InterproceduralAnalysisConfiguration.Create(
			analyzerOptions, rule, cfg, wellKnownTypeProvider.Compilation, interproceduralAnalysisKind);
		var completionOwnershipTransferAtConstructor = analyzerOptions.GetCompletionOwnershipTransferAtConstructorOption(
			rule, owningSymbol, wellKnownTypeProvider.Compilation, defaultValue: defaultCompletionOwnershipTransferAtConstructor);
		var completionOwnershipTransferAtMethodCall = analyzerOptions.GetCompletionOwnershipTransferAtMethodCall(
			rule, owningSymbol, wellKnownTypeProvider.Compilation, defaultValue: defaultCompletionOwnershipTransferAtMethodCall);

		if (!CompleteOnNormalPathAnalysisHelper.TryGetOrCreate(wellKnownTypeProvider.Compilation, targetTypeFullName, completeMethodName, out var completeOnNormalPathAnalysisHelper))
		{
			// Failed to resolve target type or complete method
			pointsToAnalysisResult = default;
			return null;
		}

		return
			TryGetOrComputeResult(
				cfg, owningSymbol, analyzerOptions, wellKnownTypeProvider, interproceduralAnalysisConfig, interproceduralAnalysisPredicate,
				completeOnNormalPathAnalysisHelper.TargetType!, completeOnNormalPathAnalysisHelper.CompleteMethod!,
				completionOwnershipTransferAtConstructor, completionOwnershipTransferAtMethodCall, trackInstanceFields,
				pointsToAnalysisKind:
					analyzerOptions.GetPointsToAnalysisKindOption(
						rule, owningSymbol, wellKnownTypeProvider.Compilation, defaultPointsToAnalysisKind
				),
				performCopyAnalysis:
					analyzerOptions.GetCopyAnalysisOption(
						rule, owningSymbol, wellKnownTypeProvider.Compilation, defaultValue: performCopyAnalysisIfNotUserConfigured),
				out pointsToAnalysisResult
			);
	}

	private static CompleteOnNormalPathAnalysisResult? TryGetOrComputeResult(
		ControlFlowGraph cfg,
		ISymbol owningSymbol,
		AnalyzerOptions analyzerOptions,
		WellKnownTypeProvider wellKnownTypeProvider,
		InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
		InterproceduralAnalysisPredicate? interproceduralAnalysisPredicate,
		INamedTypeSymbol targetType,
		IMethodSymbol completeMethod,
		bool completionOwnershipTransferAtConstructor,
		bool completionOwnershipTransferAtMethodCall,
		bool trackInstanceFields,
		PointsToAnalysisKind pointsToAnalysisKind,
		bool performCopyAnalysis,
		out PointsToAnalysisResult? pointsToAnalysisResult
	)
	{
		Debug.Assert(wellKnownTypeProvider.TryGetOrCreateTypeByMetadataName(WellKnownTypeNames.SystemIDisposable, out _));

		pointsToAnalysisResult =
			PointsToAnalysis.TryGetOrComputeResult(
				cfg, owningSymbol, analyzerOptions, wellKnownTypeProvider, pointsToAnalysisKind, interproceduralAnalysisConfig,
				interproceduralAnalysisPredicate, PessimisticAnalysis, performCopyAnalysis, exceptionPathsAnalysis: false
			);
		if (pointsToAnalysisResult == null)
		{
			return null;
		}

		var analysisContext =
			CompleteOnNormalPathAnalysisContext.Create(
				CompleteOnNormalPathAbstractValueDomain.Default, wellKnownTypeProvider, cfg, owningSymbol, analyzerOptions, interproceduralAnalysisConfig,
				interproceduralAnalysisPredicate, PessimisticAnalysis, pointsToAnalysisResult, TryGetOrComputeResultForAnalysisContext,
				targetType, completeMethod,
				completionOwnershipTransferAtConstructor, completionOwnershipTransferAtMethodCall, trackInstanceFields
			);
		return TryGetOrComputeResultForAnalysisContext(analysisContext);
	}

	private static CompleteOnNormalPathAnalysisResult? TryGetOrComputeResultForAnalysisContext(CompleteOnNormalPathAnalysisContext analysisContext)
	{
		var operationVisitor = new CompleteOnNormalPathDataFlowOperationVisitor(analysisContext);
		var analysis = new CompleteOnNormalPathAnalysis(CompleteOnNormalPathAnalysisDomainInstance, operationVisitor);
		return analysis.TryGetOrComputeResultCore(analysisContext, cacheResult: false);
	}

	protected override CompleteOnNormalPathAnalysisResult ToResult(CompleteOnNormalPathAnalysisContext analysisContext, DataFlowAnalysisResult<CompleteOnNormalPathBlockAnalysisResult, CompleteOnNormalPathAbstractValue> dataFlowAnalysisResult)
	{
		var operationVisitor = (CompleteOnNormalPathDataFlowOperationVisitor)OperationVisitor;
		var trackedInstanceFieldPointsToMap =
			analysisContext.TrackInstanceFields
			? operationVisitor.TrackedInstanceFieldPointsToMap
			: null;
		return new CompleteOnNormalPathAnalysisResult(dataFlowAnalysisResult, trackedInstanceFieldPointsToMap);
	}

	protected override CompleteOnNormalPathBlockAnalysisResult ToBlockResult(BasicBlock basicBlock, CompleteOnNormalPathAnalysisData blockAnalysisData)
		=> new(basicBlock, blockAnalysisData);
}
