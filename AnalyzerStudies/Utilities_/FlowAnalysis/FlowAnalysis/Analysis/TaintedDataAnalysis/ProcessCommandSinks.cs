// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class ProcessCommandSinks
	{
		/// <summary>
		/// <see cref="SinkInfo"/>s for tainted data process command sinks.
		/// </summary>
		public static ImmutableHashSet<SinkInfo> SinkInfos { get; }

		static ProcessCommandSinks()
		{
			var builder = PooledHashSet<SinkInfo>.GetInstance();

			builder.AddSinkInfo(
				WellKnownTypeNames.SystemDiagnosticsProcess,
				SinkKind.ProcessCommand,
				isInterface: false,
				isAnyStringParameterInConstructorASink: false,
				sinkProperties: null,
				sinkMethodParameters: new[] {
					( "Start", new[] { "fileName", "arguments", } ),
				});
			builder.AddSinkInfo(
				WellKnownTypeNames.SystemDiagnosticsProcessStartInfo,
				SinkKind.ProcessCommand,
				isInterface: false,
				isAnyStringParameterInConstructorASink: true,
				sinkProperties: new[] {
					"ArgumentList",
					"Arguments",
					"FileName",
				},
				sinkMethodParameters: null);

			SinkInfos = builder.ToImmutableAndFree();
		}
	}
}
