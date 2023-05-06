// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.DisposeAnalysis
{
	/// <summary>
	/// Analysis result from execution of <see cref="DisposeAnalysis"/> on a control flow graph.
	/// </summary>
	public sealed class DisposeAnalysisResult : DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue>
	{
		internal DisposeAnalysisResult(
			DataFlowAnalysisResult<DisposeBlockAnalysisResult, DisposeAbstractValue> coreDisposeAnalysisResult,
			ImmutableDictionary<IFieldSymbol, PointsToAnalysis.PointsToAbstractValue>? trackedInstanceFieldPointsToMap)
			: base(coreDisposeAnalysisResult)
		{
			TrackedInstanceFieldPointsToMap = trackedInstanceFieldPointsToMap;
		}

		public ImmutableDictionary<IFieldSymbol, PointsToAnalysis.PointsToAbstractValue>? TrackedInstanceFieldPointsToMap { get; }
	}
}
