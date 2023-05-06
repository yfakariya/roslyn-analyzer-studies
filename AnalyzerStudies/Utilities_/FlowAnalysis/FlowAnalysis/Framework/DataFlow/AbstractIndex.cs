// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	/// <summary>
	/// Represents an abstract index into a location.
	/// It is used by an <see cref="AnalysisEntity"/> for operations such as an <see cref="Operations.IArrayElementReferenceOperation"/>, index access <see cref="Operations.IPropertyReferenceOperation"/>, etc.
	/// </summary>
	public abstract partial class AbstractIndex : CacheBasedEquatable<AbstractIndex>
	{
		public static AbstractIndex Create(int index) => new ConstantValueIndex(index);
		public static AbstractIndex Create(AnalysisEntity analysisEntity) => new AnalysisEntityBasedIndex(analysisEntity);
		public static AbstractIndex Create(IOperation operation) => new OperationBasedIndex(operation);
	}
}
