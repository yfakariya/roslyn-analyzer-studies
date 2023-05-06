// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

namespace AnalyzerStudies.CompletionOnNormalPathAnalysis;

internal enum CompletionAnalysisKind
{
	// NOTE: Below fields names are used in the .editorconfig specification
	//       for CompleteOnNormalPathAnalysisKind option. Hence the names should *not* be modified,
	//       as that would be a breaking change for .editorconfig specification.

	/// <summary>
	/// Track and report missing completion violations only on non-exception program paths.
	/// Additionally, also flag use of non-recommended completion patterns that may cause
	/// potential completion leaks.
	/// </summary>
	NonExceptionPaths,

	/// <summary>
	/// Track and report missing completion violations only on non-exception program paths.
	/// Do not flag use of non-recommended completion patterns that may cause
	/// potential completion leaks.
	/// </summary>
	NonExceptionPathsOnlyNotCompleted,
}

internal static class CompletionAnalysisKindExtensions
{
	public static bool AreExceptionPathsAndMayBeNotCompletedViolationsEnabled(this CompletionAnalysisKind analysisKind)
		=> analysisKind.AreExceptionPathsEnabled() && analysisKind.AreMayBeNotCompletedViolationsEnabled();

	public static bool AreExceptionPathsEnabled(this CompletionAnalysisKind _)
		=> false;

	public static bool AreMayBeNotCompletedViolationsEnabled(this CompletionAnalysisKind analysisKind)
		=> analysisKind switch
		{
			CompletionAnalysisKind.NonExceptionPathsOnlyNotCompleted => false,
			_ => true,
		};
}
