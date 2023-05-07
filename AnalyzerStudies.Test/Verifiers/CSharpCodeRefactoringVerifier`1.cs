// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;

namespace AnalyzerStudies.Test
{
	public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
		where TCodeRefactoring : CodeRefactoringProvider, new()
	{
		/// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(System.String, System.String)"/>
		public static async Task VerifyRefactoringAsync(string source, string fixedSource) => await VerifyRefactoringAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource).ConfigureAwait(false);

		/// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(System.String, DiagnosticResult, System.String)"/>
		public static async Task VerifyRefactoringAsync(string source, DiagnosticResult expected, string fixedSource) => await VerifyRefactoringAsync(source, new[] { expected }, fixedSource).ConfigureAwait(false);

		/// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(System.String, DiagnosticResult[], System.String)"/>
		public static async Task VerifyRefactoringAsync(string source, DiagnosticResult[] expected, string fixedSource)
		{
			var test = new Test
			{
				TestCode = source,
				FixedCode = fixedSource,
			};

			test.ExpectedDiagnostics.AddRange(expected);
			await test.RunAsync(CancellationToken.None).ConfigureAwait(false);
		}
	}
}
