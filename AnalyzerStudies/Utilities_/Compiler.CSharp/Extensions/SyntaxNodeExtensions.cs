// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Analyzer.Utilities.Extensions
{
	internal static partial class SyntaxNodeExtensions
	{
		public static SyntaxNode WalkDownParentheses(this SyntaxNode node)
		{
			SyntaxNode current = node;
			while (current.IsKind(SyntaxKind.ParenthesizedExpression) && current.ChildNodes().FirstOrDefault() is SyntaxNode expression)
			{
				current = expression;
			}

			return current;
		}
	}
}
