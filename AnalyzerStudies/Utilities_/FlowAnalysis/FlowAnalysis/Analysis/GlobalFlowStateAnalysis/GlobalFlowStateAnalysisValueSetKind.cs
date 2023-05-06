// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis
{
	internal enum GlobalFlowStateAnalysisValueSetKind
	{
		/// <summary>
		/// Unset value set.
		/// This is needed along with Empty to ensure the following merge results:
		/// Unset + Known = Known
		/// Empty + Known = Empty
		/// </summary>
		Unset,

		/// <summary>
		/// One or more known set of <see cref="IAbstractAnalysisValue"/>s.
		/// </summary>
		Known,

		/// <summary>
		/// No <see cref="IAbstractAnalysisValue"/>s.
		/// </summary>
		Empty,

		/// <summary>
		/// Unknown set of <see cref="IAbstractAnalysisValue"/>s.
		/// </summary>
		Unknown
	}
}