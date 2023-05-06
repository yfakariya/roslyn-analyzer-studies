﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Diagnostics;
using Analyzer.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.CopyAnalysis
{
	using CopyAnalysisResult = DataFlowAnalysisResult<CopyBlockAnalysisResult, CopyAbstractValue>;

	/// <summary>
	/// Dataflow analysis to track <see cref="AnalysisEntity"/> instances that share the same value.
	/// </summary>
	public partial class CopyAnalysis : ForwardDataFlowAnalysis<CopyAnalysisData, CopyAnalysisContext, CopyAnalysisResult, CopyBlockAnalysisResult, CopyAbstractValue>
	{
		private CopyAnalysis(CopyDataFlowOperationVisitor operationVisitor)
			: base(operationVisitor.AnalysisDomain, operationVisitor)
		{
		}

		public static CopyAnalysisResult? TryGetOrComputeResult(
			ControlFlowGraph cfg,
			ISymbol owningSymbol,
			AnalyzerOptions analyzerOptions,
			WellKnownTypeProvider wellKnownTypeProvider,
			InterproceduralAnalysisConfiguration interproceduralAnalysisConfig,
			InterproceduralAnalysisPredicate? interproceduralAnalysisPredicate,
			bool pessimisticAnalysis = true,
			PointsToAnalysisKind pointsToAnalysisKind = PointsToAnalysisKind.PartialWithoutTrackingFieldsAndProperties,
			bool exceptionPathsAnalysis = false)
		{
			if (cfg == null)
			{
				Debug.Fail("Expected non-null CFG");
				return null;
			}

			var pointsToAnalysisResult = pointsToAnalysisKind != PointsToAnalysisKind.None ?
				PointsToAnalysis.PointsToAnalysis.TryGetOrComputeResult(cfg, owningSymbol, analyzerOptions,
					wellKnownTypeProvider, pointsToAnalysisKind, interproceduralAnalysisConfig,
					interproceduralAnalysisPredicate, pessimisticAnalysis, performCopyAnalysis: false, exceptionPathsAnalysis) :
				null;
			var analysisContext = CopyAnalysisContext.Create(CopyAbstractValueDomain.Default, wellKnownTypeProvider,
				cfg, owningSymbol, analyzerOptions, interproceduralAnalysisConfig, pessimisticAnalysis, exceptionPathsAnalysis, pointsToAnalysisResult,
				TryGetOrComputeResultForAnalysisContext, interproceduralAnalysisPredicate);
			return TryGetOrComputeResultForAnalysisContext(analysisContext);
		}

		private static CopyAnalysisResult? TryGetOrComputeResultForAnalysisContext(CopyAnalysisContext analysisContext)
		{
			var operationVisitor = new CopyDataFlowOperationVisitor(analysisContext);
			var copyAnalysis = new CopyAnalysis(operationVisitor);
			return copyAnalysis.TryGetOrComputeResultCore(analysisContext, cacheResult: true);
		}

		protected override CopyAnalysisResult ToResult(CopyAnalysisContext analysisContext, CopyAnalysisResult dataFlowAnalysisResult) => dataFlowAnalysisResult;
		protected override CopyBlockAnalysisResult ToBlockResult(BasicBlock basicBlock, CopyAnalysisData blockAnalysisData)
			=> new(basicBlock, blockAnalysisData);
	}
}
