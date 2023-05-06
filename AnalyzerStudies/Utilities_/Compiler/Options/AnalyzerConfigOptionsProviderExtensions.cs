﻿// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

#if CODEANALYSIS_V3_OR_BETTER

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer.Utilities
{
	internal static class AnalyzerConfigOptionsProviderExtensions
	{
		public static bool IsEmpty(this AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider)
		{
			// Reflection based optimization for empty analyzer config options.
			// Ideally 'AnalyzerConfigOptionsProvider.IsEmpty' would be exposed in the API.
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
			return analyzerConfigOptionsProvider.GetType().GetField("_treeDict", flags)?.GetValue(analyzerConfigOptionsProvider) is ImmutableDictionary<object, AnalyzerConfigOptions> perTreeOptionsMap
				&& perTreeOptionsMap.IsEmpty;
		}
	}
}

#endif