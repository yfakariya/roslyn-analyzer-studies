// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

using System;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer.Utilities.Lightup
{
	internal static class IUsingOperationExtensions
	{
		private static readonly Func<IUsingOperation, bool> s_isAsynchronous
			= LightupHelpers.CreateOperationPropertyAccessor<IUsingOperation, bool>(typeof(IUsingOperation), nameof(IsAsynchronous), fallbackResult: false);

		public static bool IsAsynchronous(this IUsingOperation usingOperation)
			=> s_isAsynchronous(usingOperation);
	}
}

#endif
