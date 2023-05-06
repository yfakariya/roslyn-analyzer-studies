﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis
{
	using CopyAnalysisResult = DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue>;

	public partial class CopyAnalysis : ForwardDataFlowAnalysis<CopyAnalysisData, CopyAnalysisContext, CopyAnalysisResult, CopyBlockAnalysisResult, CopyAbstractValue>
	{
		/// <summary>
		/// Predicate kind for <see cref="CopyDataFlowOperationVisitor.SetAbstractValue(CopyAnalysisData, AnalysisEntity, CopyAbstractValue, System.Func{AnalysisEntity, CopyAbstractValue}, SetCopyAbstractValuePredicateKind?, bool)"/>
		/// to indicte if the copy equality check for the SetAbstractValue call is a reference compare or a value compare operation.
		/// </summary>
		internal enum SetCopyAbstractValuePredicateKind
		{
			ValueCompare,
			ReferenceCompare
		}
	}
}
