// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities.Lightup
{
	internal static class OperationKindEx
	{
		public const OperationKind UsingDeclaration = (OperationKind)0x6c;
		public const OperationKind FunctionPointerInvocation = (OperationKind)0x78;
		public const OperationKind ImplicitIndexerReference = (OperationKind)0x7b;
		public const OperationKind Attribute = (OperationKind)0x7d;
	}
}

#endif
