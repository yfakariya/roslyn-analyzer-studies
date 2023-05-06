// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Concurrent;

namespace Analyzer.Utilities
{
	/// <summary>
	/// A placeholder value type for <see cref="ConcurrentDictionary{TKey, TValue}"/> used as a set.
	/// </summary>
	internal readonly struct UnusedValue
	{
	}
}
