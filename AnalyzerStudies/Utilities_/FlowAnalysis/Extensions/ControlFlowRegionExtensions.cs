// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.FlowAnalysis
{
	internal static class ControlFlowRegionExtensions
	{
		public static bool ContainsRegionOrSelf(this ControlFlowRegion controlFlowRegion, ControlFlowRegion nestedRegion)
			=> controlFlowRegion.FirstBlockOrdinal <= nestedRegion.FirstBlockOrdinal &&
			controlFlowRegion.LastBlockOrdinal >= nestedRegion.LastBlockOrdinal;

		public static IEnumerable<IOperation> DescendantOperations(this ControlFlowRegion controlFlowRegion, ControlFlowGraph cfg)
		{
			for (var i = controlFlowRegion.FirstBlockOrdinal; i <= controlFlowRegion.LastBlockOrdinal; i++)
			{
				var block = cfg.Blocks[i];
				foreach (var operation in block.DescendantOperations())
				{
					yield return operation;
				}
			}
		}
	}
}
