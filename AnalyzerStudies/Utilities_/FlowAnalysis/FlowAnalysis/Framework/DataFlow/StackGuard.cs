// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	/// <summary>
	/// Stack guard for <see cref="DataFlowOperationVisitor{TAnalysisData, TAnalysisContext, TAnalysisResult, TAbstractAnalysisValue}"/> to ensure sufficient stack while recursively visiting the operation tree.
	/// </summary>
	internal static class StackGuard
	{
		public const int MaxUncheckedRecursionDepth = 20;

		public static void EnsureSufficientExecutionStack(int recursionDepth)
		{
			if (recursionDepth > MaxUncheckedRecursionDepth)
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
			}
		}

		public static bool IsInsufficientExecutionStackException(Exception ex)
		{
			return ex.GetType().Name == "InsufficientExecutionStackException";
		}
	}
}