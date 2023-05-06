// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis
{
	/// <summary>
	/// Abstract null value for <see cref="AnalysisEntity"/>/<see cref="IOperation"/> tracked by <see cref="PointsToAnalysis"/>.
	/// </summary>
	public enum NullAbstractValue
	{
		Invalid,
		Undefined,
		Null,
		NotNull,
		MaybeNull
	}
}
