﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Microsoft.CodeAnalysis.CodeMetrics
{
	public sealed class CodeMetricsAnalysisContext
	{
		private readonly ConcurrentDictionary<SyntaxTree, SemanticModel> _semanticModelMap;

		public CodeMetricsAnalysisContext(Compilation compilation, CancellationToken cancellationToken,
			Func<INamedTypeSymbol, bool>? isExcludedFromInheritanceCountFunc = null)
		{
			Compilation = compilation;
			CancellationToken = cancellationToken;
			_semanticModelMap = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
			IsExcludedFromInheritanceCountFunc = isExcludedFromInheritanceCountFunc ?? (x => false); // never excluded by default
		}

		public Compilation Compilation { get; }
		public CancellationToken CancellationToken { get; }
		public Func<INamedTypeSymbol, bool> IsExcludedFromInheritanceCountFunc { get; }

		internal SemanticModel GetSemanticModel(SyntaxNode node)
			=> _semanticModelMap.GetOrAdd(node.SyntaxTree, tree => Compilation.GetSemanticModel(node.SyntaxTree));
	}
}

#endif
