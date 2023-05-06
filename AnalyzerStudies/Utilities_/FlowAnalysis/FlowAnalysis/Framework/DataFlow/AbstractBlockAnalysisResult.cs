// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	/// <summary>
	/// Result from execution of a <see cref="DataFlowAnalysis"/> on a basic block.
	/// </summary>
	public abstract class AbstractBlockAnalysisResult
	{
		protected AbstractBlockAnalysisResult(BasicBlock basicBlock)
		{
			BasicBlock = basicBlock;
		}

		public BasicBlock BasicBlock { get; }
	}
}
