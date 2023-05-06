﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class LdapSinks
	{
		/// <summary>
		/// <see cref="SinkInfo"/>s for tainted data LDAP injection sinks.
		/// </summary>
		public static ImmutableHashSet<SinkInfo> SinkInfos { get; }

		static LdapSinks()
		{
			var builder = PooledHashSet<SinkInfo>.GetInstance();

			builder.AddSinkInfo(
				WellKnownTypeNames.SystemDirectoryServicesActiveDirectoryADSearcher,
				SinkKind.Ldap,
				isInterface: false,
				isAnyStringParameterInConstructorASink: true,
				sinkProperties: new[] {
					"Filter",
				},
				sinkMethodParameters: null);
			builder.AddSinkInfo(
				WellKnownTypeNames.SystemDirectoryServicesDirectorySearcher,
				SinkKind.Ldap,
				isInterface: false,
				isAnyStringParameterInConstructorASink: true,
				sinkProperties: new[] {
					"Filter",
				},
				sinkMethodParameters: null);
			builder.AddSinkInfo(
				WellKnownTypeNames.SystemDirectoryDirectoryEntry,
				SinkKind.Ldap,
				isInterface: false,
				isAnyStringParameterInConstructorASink: false,
				sinkProperties: null,
				sinkMethodParameters: new[] {
					(".ctor", new[] { "path", "adsObject" } ),
				});

			SinkInfos = builder.ToImmutableAndFree();
		}
	}
}
