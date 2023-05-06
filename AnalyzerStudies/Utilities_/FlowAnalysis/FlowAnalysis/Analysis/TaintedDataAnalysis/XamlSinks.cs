// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class XamlSinks
	{
		/// <summary>
		/// <see cref="SinkInfo"/>s for tainted data XAML injection sinks.
		/// </summary>
		public static ImmutableHashSet<SinkInfo> SinkInfos { get; }

		static XamlSinks()
		{
			var builder = PooledHashSet<SinkInfo>.GetInstance();

			builder.AddSinkInfo(
				WellKnownTypeNames.SystemWindowsMarkupXamlReader,
				SinkKind.Xaml,
				isInterface: false,
				isAnyStringParameterInConstructorASink: false,
				sinkProperties: null,
				sinkMethodParameters: new[] {
					( "Load", new[] { "stream", "reader", "xaml" }),
					( "LoadAsync", new[] { "stream", "reader" }),
					( "LoadWithInitialTemplateValidation", new[] { "xaml" }),
				});

			SinkInfos = builder.ToImmutableAndFree();
		}
	}
}