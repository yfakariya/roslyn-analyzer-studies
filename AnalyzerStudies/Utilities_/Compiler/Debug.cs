// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Analyzer.Utilities
{
	internal static class RoslynDebug
	{
		/// <inheritdoc cref="Debug.Assert(bool)"/>
		[Conditional("DEBUG")]
		public static void Assert([DoesNotReturnIf(false)] bool b) => Debug.Assert(b);

		/// <inheritdoc cref="Debug.Assert(bool, string)"/>
		[Conditional("DEBUG")]
		public static void Assert([DoesNotReturnIf(false)] bool b, string message)
			=> Debug.Assert(b, message);
	}
}
