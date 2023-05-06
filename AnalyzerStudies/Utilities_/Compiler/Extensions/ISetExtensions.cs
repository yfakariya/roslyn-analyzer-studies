// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace System.Collections.Generic
{
	internal static class ISetExtensions
	{
		public static void AddRange<T>(this ISet<T> set, IEnumerable<T> values)
		{
			foreach (var value in values)
			{
				set.Add(value);
			}
		}
	}
}