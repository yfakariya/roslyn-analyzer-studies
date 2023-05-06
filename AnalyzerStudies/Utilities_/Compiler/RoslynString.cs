// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace Analyzer.Utilities
{
	internal static class RoslynString
	{
		/// <inheritdoc cref="string.IsNullOrEmpty(string)"/>
		public static bool IsNullOrEmpty([NotNullWhen(returnValue: false)] string? value)
			=> string.IsNullOrEmpty(value);

#if !NET20
		/// <inheritdoc cref="string.IsNullOrWhiteSpace(string)"/>
		public static bool IsNullOrWhiteSpace([NotNullWhen(returnValue: false)] string? value)
			=> string.IsNullOrWhiteSpace(value);
#endif
	}
}
