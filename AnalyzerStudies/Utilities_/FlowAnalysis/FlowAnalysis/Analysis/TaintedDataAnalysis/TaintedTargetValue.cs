// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class TaintedTargetValue
	{
		/// <summary>
		/// Taint return value.
		/// </summary>
		public const string Return = ".Return";

		/// <summary>
		/// Taint the instance value.
		/// </summary>
		public const string This = ".This";
	}
}
