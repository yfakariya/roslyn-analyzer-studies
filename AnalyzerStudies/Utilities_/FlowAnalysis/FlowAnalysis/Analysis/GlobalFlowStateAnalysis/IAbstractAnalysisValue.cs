// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.GlobalFlowStateAnalysis
{
	/// <summary>
	/// Abstract analysis value for <see cref="GlobalFlowStateAnalysis"/>.
	/// </summary>
	internal interface IAbstractAnalysisValue : IEquatable<IAbstractAnalysisValue>
	{
		/// <summary>
		/// Return negated value if the analysis value is a predicated value.
		/// Otherwise, return the current instance itself.
		/// </summary>
		/// <returns></returns>
		IAbstractAnalysisValue GetNegatedValue();

		/// <summary>
		/// String representation of the abstract value.
		/// </summary>
		/// <returns></returns>
		string ToString();
	}
}
