﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class RegexSinks
	{
		/// <summary>
		/// <see cref="SinkInfo"/>s for tainted data process command sinks.
		/// </summary>
		public static ImmutableHashSet<SinkInfo> SinkInfos { get; }

		static RegexSinks()
		{
			var builder = PooledHashSet<SinkInfo>.GetInstance();

			builder.AddSinkInfo(
				WellKnownTypeNames.SystemTextRegularExpressionsRegex,
				SinkKind.Regex,
				isInterface: false,
				isAnyStringParameterInConstructorASink: true,
				sinkProperties: null,
				sinkMethodParameters: new[] {
					( "IsMatch", new[] { "pattern" }),
					( "Match", new[] { "pattern" }),
					( "Matches", new[] { "pattern" }),
					( "Replace", new[] { "pattern" }),
					( "Split", new[] { "pattern" }),
				});

			SinkInfos = builder.ToImmutableAndFree();
		}
	}
}
