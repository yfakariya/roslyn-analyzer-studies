// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using Analyzer.Utilities;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	public abstract partial class AbstractIndex
	{
		private sealed class AnalysisEntityBasedIndex : AbstractIndex
		{
			public AnalysisEntityBasedIndex(AnalysisEntity analysisEntity)
			{
				AnalysisEntity = analysisEntity;
			}

			public AnalysisEntity AnalysisEntity { get; }

			protected override void ComputeHashCodeParts(ref RoslynHashCode hashCode)
			{
				hashCode.Add(AnalysisEntity.GetHashCode());
				hashCode.Add(nameof(AnalysisEntityBasedIndex).GetHashCode());
			}

			protected override bool ComputeEqualsByHashCodeParts(CacheBasedEquatable<AbstractIndex> obj)
			{
				var other = (AnalysisEntityBasedIndex)obj;
				return AnalysisEntity.GetHashCode() == other.AnalysisEntity.GetHashCode();
			}
		}
	}
}
