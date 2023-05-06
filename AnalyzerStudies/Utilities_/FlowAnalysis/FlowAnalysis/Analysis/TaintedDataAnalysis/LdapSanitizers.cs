// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Collections.Immutable;
using Analyzer.Utilities.PooledObjects;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.TaintedDataAnalysis
{
	internal static class LdapSanitizers
	{
		/// <summary>
		/// <see cref="SanitizerInfo"/>s for LDAP injection sanitizers.
		/// </summary>
		public static ImmutableHashSet<SanitizerInfo> SanitizerInfos { get; }

		static LdapSanitizers()
		{
			var builder = PooledHashSet<SanitizerInfo>.GetInstance();

			builder.AddSanitizerInfo(
				WellKnownTypeNames.MicrosoftSecurityApplicationEncoder,
				isInterface: false,
				isConstructorSanitizing: false,
				sanitizingMethods: new[] {
					"LdapDistinguishedNameEncode",
					"LdapEncode",
					"LdapFilterEncode",
				});

			SanitizerInfos = builder.ToImmutableAndFree();
		}
	}
}
