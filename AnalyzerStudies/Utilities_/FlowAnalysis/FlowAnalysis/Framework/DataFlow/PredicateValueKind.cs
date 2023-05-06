// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	public enum PredicateValueKind
	{
		/// <summary>
		/// Predicate always evaluates to true.
		/// </summary>
		AlwaysTrue,

		/// <summary>
		/// Predicate always evaluates to false.
		/// </summary>
		AlwaysFalse,

		/// <summary>
		/// Predicate might evaluate to true or false.
		/// </summary>
		Unknown
	}
}
