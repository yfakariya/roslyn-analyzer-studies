# Analyzer Studies

## Directories

* `CompletionOnNormalPathAnalysis`: Data flow analysis logics for `CompleteObjectBeforeLosingScope` analizer, based on data flow analysis framework of Roslyn Analyzer.
* `Utilities`: Contains data flow analysis framework of Roslyn Analyzer.

## Analyzers

* `CompleteObjectBeforeLosingScope`: Analyzer to detect missing "Complete" call on object before losing scope.

### CompleteObjectBeforeLosingScope

This analyzer detects missing "Complete" call on object before losing scope.
It is abstract class and takes three parameters: 1) rule ID, 2) target type full name (including its namespace), and 3) "Complete" method name.
For example, if you detect missing `Complete` call of `System.Transactions.TransactionScope`, derived from this class as [source in test project](../AnalyzerStudies.Test/CompleteTransactionScopeBeforeLosingScope.cs).

This analyzer is based on data flow analysis framework of Roslyn Analyzer, especially `DisposeAnalysis` and `DisposeObjectBeforeLosingScope` analyzer.
