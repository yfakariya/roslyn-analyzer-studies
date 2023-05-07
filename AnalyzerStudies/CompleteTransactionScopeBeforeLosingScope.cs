// Copyright (c) FUJIWARA, Yusuke and all contributors.
// This file is licensed under MIT license.
// See the LICENSE in the project root for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AnalyzerStudies;

/// <summary>
/// Implementatino of <see cref="CompleteObjectBeforeLosingScope"/> for testing.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class CompleteTransactionScopeBeforeLosingScope : CompleteObjectBeforeLosingScope
{
	public CompleteTransactionScopeBeforeLosingScope()
		: base("XA0001", "System.Transactions.TransactionScope", "Complete") { }
}
