// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

using CompleteOnNormalPathAnalysisData =
	Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DictionaryAnalysisData<
		Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.AbstractLocation,
		AnalyzerStudies.CompletionOnNormalPathAnalysis.CompleteOnNormalPathAbstractValue>;

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

/// <summary>
/// Result from execution of <see cref="CompleteOnNormalPathAnalysis"/> on a basic block.
/// It store target completion values for each <see cref="AbstractLocation"/> at the start and end of the basic block.
/// </summary>
public sealed class CompleteOnNormalPathBlockAnalysisResult : AbstractBlockAnalysisResult
{
	public ImmutableDictionary<AbstractLocation, CompleteOnNormalPathAbstractValue> Data { get; }

	internal CompleteOnNormalPathBlockAnalysisResult(
		BasicBlock basicBlock,
		CompleteOnNormalPathAnalysisData? blockAnalysisData
	) : base(basicBlock)
	{
		Data = blockAnalysisData?.ToImmutableDictionary() ??
			ImmutableDictionary<AbstractLocation, CompleteOnNormalPathAbstractValue>.Empty;
	}
}