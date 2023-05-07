# Analyzer Studies NuGet Package

## Usage

Build this project and pick up the `*.nupkg` file in `bin` directory. Then, upload it to your preferred NuGet feed.

**Before building, you need to set package related properties in `AnalyzerStudies.Package.csproj` file.** The properties will be found in `<PropertyGroup>` element at the top of `.csproj` file.

## Add CodeFix providers

Currently, this project does not depend on `AnalyzerStudies.CodeFixes` project. If you want to add CodeFix providers, you need to add a dependency to `AnalyzerStudies.CodeFixes` project in `AnalyzerStudies.Package.csproj` file and change `.csproj` to pick up the dll.
To do so, uncomment `<ProjectReference>` element and `<TfmSpecificPackageFile>` element in `.csproj` file.
