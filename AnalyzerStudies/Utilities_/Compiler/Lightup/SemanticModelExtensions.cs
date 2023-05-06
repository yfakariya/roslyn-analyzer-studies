// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities.Lightup
{
	internal static class SemanticModelExtensions
	{
		private static readonly Func<SemanticModel, int, NullableContext> s_getNullableContext
			= LightupHelpers.CreateAccessorWithArgument<SemanticModel, int, NullableContext>(typeof(SemanticModel), "semanticModel", typeof(int), "position", nameof(GetNullableContext), fallbackResult: NullableContext.Disabled);

		public static NullableContext GetNullableContext(this SemanticModel semanticModel, int position)
			=> s_getNullableContext(semanticModel, position);
	}
}
