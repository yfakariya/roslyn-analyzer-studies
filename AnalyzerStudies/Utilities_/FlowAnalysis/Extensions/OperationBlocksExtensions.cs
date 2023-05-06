// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Analyzer.Utilities.Extensions
{
	internal static partial class OperationBlocksExtensions
	{
		public static ControlFlowGraph? GetControlFlowGraph(this ImmutableArray<IOperation> operationBlocks)
		{
			foreach (var operationRoot in operationBlocks)
			{
				if (operationRoot.TryGetEnclosingControlFlowGraph(out var cfg))
				{
					return cfg;
				}
			}

			return null;
		}
	}
}
