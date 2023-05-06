// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class AnySanitizers
	{
		/// <summary>
		/// <see cref="SanitizerInfo"/>s for any tainted data sanitizers.
		/// </summary>
		public static ImmutableHashSet<SanitizerInfo> SanitizerInfos { get; }

		static AnySanitizers()
		{
			var builder = PooledHashSet<SanitizerInfo>.GetInstance();

			builder.AddSanitizerInfo(
				WellKnownTypeNames.SystemTextStringBuilder,
				isInterface: false,
				isConstructorSanitizing: false,
				sanitizingMethods: default(string[]),
				sanitizingInstanceMethods: new[] {
					"Clear",
				});

			SanitizerInfos = builder.ToImmutableAndFree();
		}
	}
}
