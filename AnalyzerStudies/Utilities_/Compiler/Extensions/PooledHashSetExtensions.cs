// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Generic;

namespace Analyzer.Utilities.PooledObjects.Extensions
{
	internal static class PooledHashSetExtensions
	{
		public static void AddRange<T>(this PooledHashSet<T> builder, IEnumerable<T> set2)
		{
			foreach (var item in set2)
			{
				builder.Add(item);
			}
		}
	}
}