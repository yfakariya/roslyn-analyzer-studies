// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;

namespace Microsoft.CodeAnalysis.FlowAnalysis.DataFlow
{
	public abstract class AbstractAnalysisData : IDisposable
	{
		public bool IsDisposed { get; private set; }

		protected virtual void Dispose(bool disposing)
		{
			IsDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
