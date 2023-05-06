// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerStudies;

// TODO: Remove
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AnalyzerStudiesAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

	public override void Initialize(AnalysisContext context)
	{
		throw new NotImplementedException();
	}
}
