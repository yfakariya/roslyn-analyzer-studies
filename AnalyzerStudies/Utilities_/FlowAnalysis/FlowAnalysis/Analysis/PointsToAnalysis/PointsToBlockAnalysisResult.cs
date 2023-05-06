﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis
{
	/// <summary>
	/// Result from execution of <see cref="PointsToAnalysis"/> on a basic block.
	/// It stores the PointsTo value for each <see cref="AnalysisEntity"/> at the start and end of the basic block.
	/// </summary>
	public class PointsToBlockAnalysisResult : AbstractBlockAnalysisResult
	{
		internal PointsToBlockAnalysisResult(BasicBlock basicBlock, PointsToAnalysisData blockAnalysisData)
			: base(basicBlock)
		{
			Data = blockAnalysisData?.CoreAnalysisData.ToImmutableDictionary() ?? ImmutableDictionary<AnalysisEntity, PointsToAbstractValue>.Empty;
			IsReachable = blockAnalysisData?.IsReachableBlockData ?? true;
		}

		public ImmutableDictionary<AnalysisEntity, PointsToAbstractValue> Data { get; }
		public bool IsReachable { get; }
	}
}
