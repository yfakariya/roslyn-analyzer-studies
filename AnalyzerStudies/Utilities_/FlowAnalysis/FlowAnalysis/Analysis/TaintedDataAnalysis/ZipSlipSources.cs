// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class ZipSlipSources
	{
		/// <summary>
		/// <see cref="SourceInfo"/>s for zip slip tainted data sources.
		/// </summary>
		public static ImmutableHashSet<SourceInfo> SourceInfos { get; }

		/// <summary>
		/// Statically constructs.
		/// </summary>
		static ZipSlipSources()
		{
			var sourceInfosBuilder = PooledHashSet<SourceInfo>.GetInstance();

			sourceInfosBuilder.AddSourceInfo(
				WellKnownTypeNames.SystemIOCompressionZipArchiveEntry,
				isInterface: false,
				taintedProperties: new string[] {
					"FullName",
				},
				taintedMethods: null);

			SourceInfos = sourceInfosBuilder.ToImmutableAndFree();
		}
	}
}
