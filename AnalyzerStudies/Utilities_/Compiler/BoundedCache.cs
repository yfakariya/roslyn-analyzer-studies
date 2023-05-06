// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Analyzer.Utilities
{
	/// <summary>
	/// Provides bounded cache for analyzers.
	/// Acts as a good alternative to <see cref="System.Runtime.CompilerServices.ConditionalWeakTable{TKey, TValue}"/>
	/// when the cached value has a cyclic reference to the key preventing early garbage collection of entries.
	/// </summary>
	internal sealed class BoundedCache<TKey, TValue> : BoundedCacheWithFactory<TKey, TValue>
		where TKey : class
		where TValue : new()
	{
		public TValue GetOrCreateValue(TKey key)
		{
			return GetOrCreateValue(key, CreateDefaultValue);

			// Local functions.
			static TValue CreateDefaultValue(TKey _) => new();
		}
	}
}
