// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis
{
	internal enum PropertySetAbstractValueKind
	{
		/// <summary>
		/// Doesn't matter.
		/// </summary>
		Unknown,

		/// <summary>
		/// Not flagged for badness.
		/// </summary>
		Unflagged,

		/// <summary>
		/// Flagged for badness.
		/// </summary>
		Flagged,

		/// <summary>
		/// Maybe flagged.
		/// </summary>
		MaybeFlagged,
	}
}
