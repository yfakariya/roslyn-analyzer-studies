// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using Microsoft.CodeAnalysis.CSharp;

namespace Analyzer.CSharp.Utilities.Lightup
{
	internal static class SyntaxKindEx
	{
		public const SyntaxKind InitKeyword = (SyntaxKind)8443;
		public const SyntaxKind InitAccessorDeclaration = (SyntaxKind)9060;
		public const SyntaxKind ImplicitObjectCreationExpression = (SyntaxKind)8659;
	}
}
