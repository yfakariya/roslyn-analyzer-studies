// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	/// <summary>
	/// Abstract value domain for a <see cref="DataFlowAnalysis"/> to merge and compare values.
	/// </summary>
	public abstract class AbstractValueDomain<T> : AbstractDomain<T>
	{
		/// <summary>
		/// Returns the major Unknown or MayBe top value of the domain.
		/// </summary>
		public abstract T UnknownOrMayBeValue { get; }
	}
}
