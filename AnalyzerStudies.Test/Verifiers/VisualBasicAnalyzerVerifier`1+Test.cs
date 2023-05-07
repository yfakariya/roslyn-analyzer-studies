// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace AnalyzerStudies.Test
{
	public static partial class VisualBasicAnalyzerVerifier<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		public class Test : VisualBasicAnalyzerTest<TAnalyzer, NUnitVerifier>
		{
			public Test()
			{
			}
		}
	}
}
