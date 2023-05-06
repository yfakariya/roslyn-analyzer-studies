﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class HardcodedCertificateSinks
	{
		/// <summary>
		/// <see cref="SinkInfo"/>s for tainted data process symmetric algorithm sinks.
		/// </summary>
		public static ImmutableHashSet<SinkInfo> SinkInfos { get; }

		static HardcodedCertificateSinks()
		{
			var builder = PooledHashSet<SinkInfo>.GetInstance();

			builder.AddSinkInfo(
				WellKnownTypeNames.SystemSecurityCryptographyX509CertificatesX509Certificate,
				SinkKind.HardcodedCertificate,
				isInterface: false,
				isAnyStringParameterInConstructorASink: true,
				sinkProperties: null,
				sinkMethodParameters: new[] {
					( ".ctor", new[] { "rawData", "data" }),
				});
			builder.AddSinkInfo(
				WellKnownTypeNames.SystemSecurityCryptographyX509CertificatesX509Certificate2,
				SinkKind.HardcodedCertificate,
				isInterface: false,
				isAnyStringParameterInConstructorASink: true,
				sinkProperties: null,
				sinkMethodParameters: new[] {
					( ".ctor", new[] { "rawData", "data" }),
				});

			SinkInfos = builder.ToImmutableAndFree();
		}
	}
}
