// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	using System.Collections.Immutable;
	using Microsoft.CodeAnalysis.FlowAnalysis;
	using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

	/// <summary>
	/// Result from execution of <see cref="TaintedDataAnalysis"/> on a basic block.
	/// </summary>
	internal class TaintedDataBlockAnalysisResult : AbstractBlockAnalysisResult
	{
		public ImmutableDictionary<AnalysisEntity, TaintedDataAbstractValue> Data { get; }

		public TaintedDataBlockAnalysisResult(BasicBlock basicBlock, TaintedDataAnalysisData blockAnalysisData)
			: base(basicBlock)
		{
			Data = blockAnalysisData?.CoreAnalysisData.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, TaintedDataAbstractValue>.Empty;
		}
	}
}
