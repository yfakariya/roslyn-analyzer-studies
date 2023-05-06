// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	/// <summary>
	/// Marker interface for analysis contexts for execution of <see cref="DataFlowAnalysis"/> on a control flow graph.
	/// Primarily exists for specifying constraints on analysis context type parameters.
	/// </summary>
	public interface IDataFlowAnalysisContext
	{
		ControlFlowGraph ControlFlowGraph { get; }
		ISymbol OwningSymbol { get; }
		ControlFlowGraph? GetLocalFunctionControlFlowGraph(IMethodSymbol localFunction);
		ControlFlowGraph? GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperation lambda);
	}
}
