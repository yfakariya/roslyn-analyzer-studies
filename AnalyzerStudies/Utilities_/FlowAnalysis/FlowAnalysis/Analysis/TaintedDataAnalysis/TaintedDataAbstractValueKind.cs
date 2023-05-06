// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	/// <summary>
	/// Kind for <see cref="TaintedDataAbstractValue"/>.
	/// </summary>
	internal enum TaintedDataAbstractValueKind
	{
		/// <summary>
		/// Indicates the data is definitely untainted (cuz it was sanitized).
		/// </summary>
		NotTainted,

		/// <summary>
		/// Indicates that data is definitely tainted.
		/// </summary>
		Tainted,
	}
}
