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
using CopyAnalysisResult =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DataFlowAnalysisResult<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis.CopyBlockAnalysisResult,
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis.CopyAbstractValue>;
using InterproceduralCompleteOnNormalPathAnalysisData =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.InterproceduralAnalysisData<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DictionaryAnalysisData<
			Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.AbstractLocation,
			AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAnalysisContext,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>;
using ValueContentAnalysisResult =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DataFlowAnalysisResult<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis.ValueContentBlockAnalysisResult,
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis.ValueContentAbstractValue>;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

/// <summary>
/// Analysis context for execution of <see cref="CompleteOnNormalPathAnalysis"/> on a control flow graph.
/// </summary>
public sealed class CompleteOnNormalPathAnalysisContext :
	AbstractDataFlowAnalysisContext<
		CompleteOnNormalPathAnalysisData,
		CompleteOnNormalPathAnalysisContext,
		CompleteOnNormalPathAnalysisResult,
		CompleteOnNormalPathAbstractValue>
{
	public INamedTypeSymbol TargetType { get; }

	public IMethodSymbol CompleteMethod { get; }

	public bool CompletionOwnershipTransferAtConstructor { get; }

	public bool CompletionOwnershipTransferAtMethodCall { get; }

	public bool TrackInstanceFields { get; }

	private CompleteOnNormalPathAnalysisContext(
		AbstractValueDomain<CompleteOnNormalPathAbstractValue> valueDomain,
		WellKnownTypeProvider wellKnownTypeProvider,
		ControlFlowGraph controlFlowGraph,
		ISymbol owningSymbol,
		AnalyzerOptions analyzerOptions,
		InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
		bool pessimisticAnalysis,
		PointsToAnalysisResult? pointsToAnalysisResult,
		Func<CompleteOnNormalPathAnalysisContext, CompleteOnNormalPathAnalysisResult?> tryGetOrComputeAnalysisResult,
		INamedTypeSymbol targetType,
		IMethodSymbol completeMethod,
		bool completionOwnershipTransferAtConstructor,
		bool completionOwnershipTransferAtMethodCall,
		bool trackInstanceFields,
		ControlFlowGraph? parentControlFlowGraph,
		InterproceduralCompleteOnNormalPathAnalysisData? interproceduralAnalysisData,
		InterproceduralAnalysisPredicate? interproceduralAnalysisPredicate
	) : base(valueDomain, wellKnownTypeProvider, controlFlowGraph,
			  owningSymbol, analyzerOptions, interproceduralAnalysisConfig, pessimisticAnalysis,
			  predicateAnalysis: false,
			  // Always false because we only do "normal path" analysis.
			  exceptionPathsAnalysis: false,
			  copyAnalysisResult: null,
			  pointsToAnalysisResult,
			  valueContentAnalysisResult: null,
			  tryGetOrComputeAnalysisResult,
			  parentControlFlowGraph,
			  interproceduralAnalysisData,
			  interproceduralAnalysisPredicate
		)
	{
		TargetType = targetType;
		CompleteMethod = completeMethod;
		CompletionOwnershipTransferAtConstructor = completionOwnershipTransferAtConstructor;
		CompletionOwnershipTransferAtMethodCall = completionOwnershipTransferAtMethodCall;
		TrackInstanceFields = trackInstanceFields;
	}

	internal static CompleteOnNormalPathAnalysisContext Create(
		AbstractValueDomain<CompleteOnNormalPathAbstractValue> valueDomain,
		WellKnownTypeProvider wellKnownTypeProvider,
		ControlFlowGraph controlFlowGraph,
		ISymbol owningSymbol,
		AnalyzerOptions analyzerOptions,
		InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
		InterproceduralAnalysisPredicate? interproceduralAnalysisPredicate,
		bool pessimisticAnalysis,
		PointsToAnalysisResult? pointsToAnalysisResult,
		Func<CompleteOnNormalPathAnalysisContext, CompleteOnNormalPathAnalysisResult?> tryGetOrComputeAnalysisResult,
		INamedTypeSymbol targetType,
		IMethodSymbol completeMethod,
		bool completionOwnershipTransferAtConstructor,
		bool completionOwnershipTransferAtMethodCall,
		bool trackInstanceFields
	)
	{
		return
			new CompleteOnNormalPathAnalysisContext(
				valueDomain, wellKnownTypeProvider, controlFlowGraph,
				owningSymbol, analyzerOptions, interproceduralAnalysisConfig, pessimisticAnalysis,
				pointsToAnalysisResult, tryGetOrComputeAnalysisResult,
				targetType, completeMethod,
				completionOwnershipTransferAtConstructor, completionOwnershipTransferAtMethodCall, trackInstanceFields,
				parentControlFlowGraph: null, interproceduralAnalysisData: null, interproceduralAnalysisPredicate
			);
	}

	public override CompleteOnNormalPathAnalysisContext ForkForInterproceduralAnalysis(
		IMethodSymbol invokedMethod,
		ControlFlowGraph invokedCfg,
		PointsToAnalysisResult? pointsToAnalysisResult,
		CopyAnalysisResult? copyAnalysisResult,
		ValueContentAnalysisResult? valueContentAnalysisResult,
		InterproceduralCompleteOnNormalPathAnalysisData? interproceduralAnalysisData)
	{
		Debug.Assert(pointsToAnalysisResult != null);
		Debug.Assert(copyAnalysisResult == null);
		Debug.Assert(valueContentAnalysisResult == null);

		return
			new CompleteOnNormalPathAnalysisContext(
				ValueDomain, WellKnownTypeProvider, invokedCfg,
				invokedMethod, AnalyzerOptions, InterproceduralAnalysisConfiguration, PessimisticAnalysis,
				pointsToAnalysisResult, TryGetOrComputeAnalysisResult,
				TargetType, CompleteMethod,
				CompletionOwnershipTransferAtConstructor, CompletionOwnershipTransferAtMethodCall, TrackInstanceFields,
				ControlFlowGraph, interproceduralAnalysisData, InterproceduralAnalysisPredicate
			);
	}

	protected override void ComputeHashCodePartsSpecific(ref RoslynHashCode hashCode)
	{
		hashCode.Add(TrackInstanceFields.GetHashCode());
		hashCode.Add(CompletionOwnershipTransferAtConstructor.GetHashCode());
		hashCode.Add(CompletionOwnershipTransferAtMethodCall.GetHashCode());
		hashCode.Add(TargetType.GetHashCode());
		hashCode.Add(CompleteMethod.GetHashCode());
	}

	protected override bool ComputeEqualsByHashCodeParts(AbstractDataFlowAnalysisContext<CompleteOnNormalPathAnalysisData, CompleteOnNormalPathAnalysisContext, CompleteOnNormalPathAnalysisResult, CompleteOnNormalPathAbstractValue> obj)
	{
		var other = obj as CompleteOnNormalPathAnalysisContext;
		return
			other is not null &&
			TrackInstanceFields == other.TrackInstanceFields &&
			CompletionOwnershipTransferAtConstructor == other.CompletionOwnershipTransferAtConstructor &&
			CompletionOwnershipTransferAtMethodCall == other.CompletionOwnershipTransferAtMethodCall &&
			SymbolEqualityComparer.Default.Equals(TargetType, other.TargetType) &&
			SymbolEqualityComparer.Default.Equals(CompleteMethod, other.CompleteMethod);
	}
}
