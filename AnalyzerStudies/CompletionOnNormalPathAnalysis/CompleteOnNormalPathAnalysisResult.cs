// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

/// <summary>
/// Analysis result from execution of <see cref="CompleteOnNormalPathAnalysis"/> on a control flow graph.
/// </summary>
public sealed class CompleteOnNormalPathAnalysisResult : DataFlowAnalysisResult<CompleteOnNormalPathBlockAnalysisResult, CompleteOnNormalPathAbstractValue>
{
	public ImmutableDictionary<IFieldSymbol, PointsToAbstractValue>? TrackedInstanceFieldPointsToMap { get; }

	internal CompleteOnNormalPathAnalysisResult(
		DataFlowAnalysisResult<CompleteOnNormalPathBlockAnalysisResult, CompleteOnNormalPathAbstractValue> coreCompleteOnNormalPathAnalysisResult,
		ImmutableDictionary<IFieldSymbol, PointsToAbstractValue>? trackedInstanceFieldPointsToMap
	) : base(coreCompleteOnNormalPathAnalysisResult)
	{
		TrackedInstanceFieldPointsToMap = trackedInstanceFieldPointsToMap;
	}
}
