// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

	internal partial class TaintedDataAnalysis
	{
		private sealed class TaintedDataAnalysisDomain : PredicatedAnalysisDataDomain<TaintedDataAnalysisData, TaintedDataAbstractValue>
		{
			public TaintedDataAnalysisDomain(MapAbstractDomain<AnalysisEntity, TaintedDataAbstractValue> coreDataAnalysisDomain)
				: base(coreDataAnalysisDomain)
			{
			}
		}
	}
}