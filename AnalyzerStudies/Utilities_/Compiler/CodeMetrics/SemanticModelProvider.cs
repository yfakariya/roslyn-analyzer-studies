// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

using System.Collections.Concurrent;

namespace Microsoft.CodeAnalysis.CodeMetrics
{
	internal sealed class SemanticModelProvider
	{
		private readonly ConcurrentDictionary<SyntaxTree, SemanticModel> _semanticModelMap;
		public SemanticModelProvider(Compilation compilation)
		{
			Compilation = compilation;
			_semanticModelMap = new ConcurrentDictionary<SyntaxTree, SemanticModel>();
		}

		public Compilation Compilation { get; }

		public SemanticModel GetSemanticModel(SyntaxNode node)
			=> _semanticModelMap.GetOrAdd(node.SyntaxTree, tree => Compilation.GetSemanticModel(node.SyntaxTree));
	}
}

#endif
