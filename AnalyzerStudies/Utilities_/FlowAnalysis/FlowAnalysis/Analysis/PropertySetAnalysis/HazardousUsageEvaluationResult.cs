// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis
{
	/// <summary>
	/// Result of evaluating potentially hazardous usage.
	/// </summary>
	public enum HazardousUsageEvaluationResult
	{
		/// <summary>
		/// The usage is not hazardous.
		/// </summary>
		Unflagged,

		/// <summary>
		/// The usage might be hazardous.
		/// </summary>
		MaybeFlagged,

		/// <summary>
		/// The usage is definitely hazardous.
		/// </summary>
		Flagged,
	}
}
