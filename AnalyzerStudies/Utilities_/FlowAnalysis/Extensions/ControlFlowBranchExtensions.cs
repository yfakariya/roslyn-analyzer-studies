// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis
{
	internal static class ControlFlowBranchExtensions
	{
		public static bool IsBackEdge(this ControlFlowBranch controlFlowBranch)
			=> controlFlowBranch?.Source != null &&
			   controlFlowBranch.Destination != null &&
			   controlFlowBranch.Source.Ordinal >= controlFlowBranch.Destination.Ordinal;
	}
}
