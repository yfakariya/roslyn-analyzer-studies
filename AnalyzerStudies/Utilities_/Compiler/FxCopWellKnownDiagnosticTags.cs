// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
	internal static class FxCopWellKnownDiagnosticTags
	{
		public const string PortedFromFxCop = nameof(PortedFromFxCop);

		public static readonly string[] PortedFxCopRule = new string[] { PortedFromFxCop, WellKnownDiagnosticTags.Telemetry };
		public static readonly string[] PortedFxCopRuleEnabledInAggressiveMode = new string[] { PortedFromFxCop, WellKnownDiagnosticTags.Telemetry, WellKnownDiagnosticTagsExtensions.EnabledRuleInAggressiveMode };

		public static readonly string[] PortedFxCopDataflowRule = new string[] { PortedFromFxCop, WellKnownDiagnosticTagsExtensions.Dataflow, WellKnownDiagnosticTags.Telemetry };
		public static readonly string[] PortedFxCopDataflowRuleEnabledInAggressiveMode = new string[] { PortedFromFxCop, WellKnownDiagnosticTagsExtensions.Dataflow, WellKnownDiagnosticTags.Telemetry, WellKnownDiagnosticTagsExtensions.EnabledRuleInAggressiveMode };
	}
}