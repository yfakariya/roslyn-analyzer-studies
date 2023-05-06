// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if HAS_IOPERATION

namespace Analyzer.Utilities.Lightup
{
	using Microsoft.CodeAnalysis;

	internal interface IOperationWrapper
	{
		IOperation? WrappedOperation { get; }
	}
}

#endif
