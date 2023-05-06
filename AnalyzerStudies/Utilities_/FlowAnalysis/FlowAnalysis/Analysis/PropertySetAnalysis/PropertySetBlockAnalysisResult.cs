// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis
{
	using PropertySetAnalysisData = DictionaryAnalysisData<AbstractLocation, PropertySetAbstractValue>;

	/// <summary>
	/// Result from execution of <see cref="PropertySetAnalysis"/> on a basic block.
	/// It stores <see cref="PropertySetAbstractValue"/>s for each <see cref="AbstractLocation"/> at the start and end of the basic block.
	/// </summary>
	internal class PropertySetBlockAnalysisResult : AbstractBlockAnalysisResult
	{
		public PropertySetBlockAnalysisResult(BasicBlock basicBlock, PropertySetAnalysisData blockAnalysisData)
			: base(basicBlock)
		{
			Data = blockAnalysisData?.ToImmutableDictionary() ?? ImmutableDictionary<AbstractLocation, PropertySetAbstractValue>.Empty;
		}

		public ImmutableDictionary<AbstractLocation, PropertySetAbstractValue> Data { get; }
	}
}
