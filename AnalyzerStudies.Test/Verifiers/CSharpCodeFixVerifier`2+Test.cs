// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using AnalyzerStudies.Test.Verifiers;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace AnalyzerStudies.Test
{
	public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
		where TAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
	{
		public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, NUnitVerifier>
		{
			public Test() => SolutionTransforms.Add((solution, projectId) =>
										  {
											  var compilationOptions = solution.GetProject(projectId)!.CompilationOptions;
											  compilationOptions = compilationOptions!.WithSpecificDiagnosticOptions(
												  compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
											  solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

											  return solution;
										  });
		}
	}
}
