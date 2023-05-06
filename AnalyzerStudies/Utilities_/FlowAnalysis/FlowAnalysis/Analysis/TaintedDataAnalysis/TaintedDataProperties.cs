// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class TaintedDataProperties
	{
		/// <summary>
		/// Language agnostic indexer name used in tainted data rules.
		/// </summary>
		public const string IndexerName = "this[]";
	}
}
