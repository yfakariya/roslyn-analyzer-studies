// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities.Extensions
{
	internal static class OperationKinds
	{
		public static ImmutableArray<OperationKind> MemberReference { get; }
			= ImmutableArray.Create(
				OperationKind.EventReference,
				OperationKind.FieldReference,
				OperationKind.MethodReference,
				OperationKind.PropertyReference);
	}
}

#endif
