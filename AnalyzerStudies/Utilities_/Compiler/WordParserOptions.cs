// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;

namespace Analyzer.Utilities
{
	/// <summary>
	///   Defines the word parsing and delimiting options for use with <see cref="WordParser.Parse(string,WordParserOptions)"/>.
	/// </summary>
	[Flags]
	internal enum WordParserOptions
	{
		/// <summary>
		///   Indicates the default options for word parsing.
		/// </summary>
		None = 0,

		/// <summary>
		///   Indicates that <see cref="WordParser.Parse(string,WordParserOptions)"/> should ignore the mnemonic indicator characters (&amp;) embedded within words.
		/// </summary>
		IgnoreMnemonicsIndicators = 1,

		/// <summary>
		///   Indicates that <see cref="WordParser.Parse(string,WordParserOptions)"/> should split compound words.
		/// </summary>
		SplitCompoundWords = 2,
	}
}
