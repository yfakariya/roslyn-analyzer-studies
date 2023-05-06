// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Generic;

namespace Analyzer.Utilities.Extensions
{
	internal static class KeyValuePairExtensions
	{
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
		{
			key = pair.Key;
			value = pair.Value;
		}

		public static KeyValuePair<TKey?, TValue?> AsNullable<TKey, TValue>(this KeyValuePair<TKey, TValue> pair)
		{
			// This conversion is safe
			return pair!;
		}
	}
}
