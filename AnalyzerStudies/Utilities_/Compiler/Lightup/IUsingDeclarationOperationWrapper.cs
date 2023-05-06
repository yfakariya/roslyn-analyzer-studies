// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

namespace Analyzer.Utilities.Lightup
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.Operations;

	[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not a comparable instance.")]
	internal readonly struct IUsingDeclarationOperationWrapper : IOperationWrapper
	{
		internal const string WrappedTypeName = "Microsoft.CodeAnalysis.Operations.IUsingDeclarationOperation";
		private static readonly Type? WrappedType = OperationWrapperHelper.GetWrappedType(typeof(IUsingDeclarationOperationWrapper));

		private static readonly Func<IOperation, IVariableDeclarationGroupOperation> DeclarationGroupAccessor = LightupHelpers.CreateOperationPropertyAccessor<IOperation, IVariableDeclarationGroupOperation>(WrappedType, nameof(DeclarationGroup), fallbackResult: null!);
		private static readonly Func<IOperation, bool> IsAsynchronousAccessor = LightupHelpers.CreateOperationPropertyAccessor<IOperation, bool>(WrappedType, nameof(IsAsynchronous), fallbackResult: false);

		private IUsingDeclarationOperationWrapper(IOperation operation)
		{
			WrappedOperation = operation;
		}

		public IOperation WrappedOperation { get; }
		public ITypeSymbol? Type => this.WrappedOperation.Type;
		public IVariableDeclarationGroupOperation DeclarationGroup => DeclarationGroupAccessor(WrappedOperation);
		public bool IsAsynchronous => IsAsynchronousAccessor(this.WrappedOperation);

		public static IUsingDeclarationOperationWrapper FromOperation(IOperation operation)
		{
			if (operation == null)
			{
				return default;
			}

			if (!IsInstance(operation))
			{
				throw new InvalidCastException($"Cannot cast '{operation.GetType().FullName}' to '{WrappedTypeName}'");
			}

			return new IUsingDeclarationOperationWrapper(operation);
		}

		public static bool IsInstance(IOperation operation)
		{
			return operation != null && LightupHelpers.CanWrapOperation(operation, WrappedType);
		}
	}
}

#endif
