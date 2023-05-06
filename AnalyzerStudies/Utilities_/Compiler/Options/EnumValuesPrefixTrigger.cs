// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities.Options
{
	internal enum EnumValuesPrefixTrigger
	{
		// NOTE: Below fields names are used in the .editorconfig specification.
		//       Hence the names should *not* be modified, as that would be a breaking
		//       change for .editorconfig specification.
		AnyEnumValue,
		AllEnumValues,
		Heuristic,
	}
}
