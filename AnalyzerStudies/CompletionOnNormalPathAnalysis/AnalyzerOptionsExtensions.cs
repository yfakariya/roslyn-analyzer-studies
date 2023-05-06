// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using AnalyzerStudies.CompletionOnNormalPathAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

// Merge members with original namespace
namespace Analyzer.Utilities;

internal static partial class AnalyzerOptionsExtensions
{

	public static bool GetCompletionOwnershipTransferAtConstructorOption(
		this AnalyzerOptions options,
		DiagnosticDescriptor rule,
		ISymbol symbol,
		Compilation compilation,
		bool defaultValue
	) => TryGetSyntaxTreeForOption(symbol, out var tree)
		? options.GetCompletionOwnershipTransferAtConstructorOption(rule, tree, compilation, defaultValue)
		: defaultValue;

	public static bool GetCompletionOwnershipTransferAtConstructorOption(
		this AnalyzerOptions options,
		DiagnosticDescriptor rule,
		SyntaxTree tree,
		Compilation compilation,
		bool defaultValue
	) => options.GetBoolOptionValue(EditorConfigOptionNames.CompletionOwnershipTransferAtConstructor, rule, tree, compilation, defaultValue);

	public static bool GetCompletionOwnershipTransferAtMethodCall(
		this AnalyzerOptions options,
		DiagnosticDescriptor rule,
		ISymbol symbol,
		Compilation compilation,
		bool defaultValue
	) => TryGetSyntaxTreeForOption(symbol, out var tree)
		? options.GetCompletionOwnershipTransferAtMethodCall(rule, tree, compilation, defaultValue)
		: defaultValue;

	public static bool GetCompletionOwnershipTransferAtMethodCall(
		this AnalyzerOptions options,
		DiagnosticDescriptor rule,
		SyntaxTree tree,
		Compilation compilation,
		bool defaultValue
	) => options.GetBoolOptionValue(EditorConfigOptionNames.CompletionOwnershipTransferAtMethodCall, rule, tree, compilation, defaultValue);

	public static CompletionAnalysisKind GetCompleteOnNormalPathAnalysisKindOption(
		this AnalyzerOptions options,
		DiagnosticDescriptor rule,
		ISymbol symbol,
		Compilation compilation,
		CompletionAnalysisKind defaultValue
	) => TryGetSyntaxTreeForOption(symbol, out var tree)
		? options.GetCompleteOnNormalPathAnalysisKindOption(rule, tree, compilation, defaultValue)
		: defaultValue;

	public static CompletionAnalysisKind GetCompleteOnNormalPathAnalysisKindOption(
		this AnalyzerOptions options,
		DiagnosticDescriptor rule,
		SyntaxTree tree,
		Compilation compilation,
		CompletionAnalysisKind defaultValue
	) => options.GetNonFlagsEnumOptionValue(EditorConfigOptionNames.DisposeAnalysisKind, rule, tree, compilation, defaultValue);
}
