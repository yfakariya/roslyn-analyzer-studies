# Analyzer Studies

## Usage

* Add your "CompleteObjectBeforeLosingScope" provider in root.
  * Delete `CompleteTransactionScopeBeforeLosingScope.cs` if you don't need it.
* Add your custom analyzer as you like.
* Add unit test in `AnalyzerStudies.Test` project if you need. Basic tests already exist in the `AnalyzerStudies.Test` project.
  * You can run tests in Visual Studio by running `AnalyzerStudies.vsix` projecct.
* Build `AnalyzerStudies.Package` project and pick up the `*.nupkg` file in `bin` directory. Then, upload it to your preferred NuGet feed.
  * Before building, you need to set package related properties in `AnalyzerStudies.Package.csproj` file.

## Directories

* `CompletionOnNormalPathAnalysis`: Data flow analysis logics for `CompleteObjectBeforeLosingScope` analizer, based on data flow analysis framework of Roslyn Analyzer.
* `Utilities`: Contains data flow analysis framework of Roslyn Analyzer.

## Analyzers

* `CompleteObjectBeforeLosingScope`: Analyzer to detect missing "Complete" call on object before losing scope.

### CompleteObjectBeforeLosingScope

This analyzer detects missing "Complete" call on object before losing scope.
It is abstract class and takes three parameters: 1) unique rule ID, 2) target type full name (including its namespace), and 3) "Complete" method name.
For example, if you detect missing `Complete` call of `System.Transactions.TransactionScope`, derived from this class as [source in test project](../AnalyzerStudies.Test/CompleteTransactionScopeBeforeLosingScope.cs).

This analyzer is based on data flow analysis framework of Roslyn Analyzer, especially `DisposeAnalysis` and `DisposeObjectBeforeLosingScope` analyzer.
