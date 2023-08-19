# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
- Dependencies - .NET 8 Preview 3
- Dependencies - Updated coverlet to 6.0.0
- Dependencies - Updated FunFair.Test.Common to 6.1.1.49
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.6.2
- Dependencies - Updated Microsoft.Extensions to 7.0.1
- Dependencies - Updated Polly to 7.2.4
- Dependencies - Updated SonarAnalyzer.CSharp to 9.4.0.72892
- SDK - Updated DotNet SDK to 8.0.100-preview.7.23376.3
- Dependencies - Updated FunFair.CodeAnalysis to 7.0.1.87
- Dependencies - Updated Meziantou.Analyzer to 2.0.82
- Dependencies - Updated Microsoft.CodeAnalysis.CSharp to 4.7.0
- Dependencies - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.7.30
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [1.2.0] - 2023-04-28
### Changed
- Changed so that OpenConnection is part of the retried part
- Dependencies - Updated FluentValidation to 11.5.2
- Dependencies - Updated Credfeto.Enumeration.Source.Generation to 1.0.9.588
- Dependencies - Updated FunFair.Test.Common to 6.0.30.633
- Dependencies - Updated Roslynator.Analyzers to 4.3.0
- Dependencies - Updated Npgsql to 7.0.4
- Dependencies - Updated SonarAnalyzer.CSharp to 9.0.0.68202
- Dependencies - Updated Meziantou.Analyzer to 2.0.43
- Switched to use ValueTask

## [1.1.2] - 2023-03-31
### Added
- Provider for SqlServer
### Fixed
- Case where DBNull wasn't explicitly checked
### Changed
- FF-1429 - Updated Meziantou.Analyzer to 2.0.19
- FF-1429 - Updated FunFair.Test.Common to 6.0.26.2754
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.54.0.64047
- FF-1429 - Updated FluentValidation to 11.5.1
- FF-1429 - Updated Microsoft.Extensions to 7.0.1
- Dependencies - Updated SonarAnalyzer.CSharp to 8.55.0.65544
- Dependencies - Updated Meziantou.Analyzer to 2.0.28

## [1.1.1] - 2023-02-26
### Changed
- FF-1429 - Updated Credfeto.Enumeration.Source.Generation to 1.0.7.19
- FF-1429 - Updated FunFair.Test.Common to 6.0.25.2731

## [1.1.0] - 2023-02-24
### Changed
- First fully working version
- Used CALL for stored procedures
- FF-1429 - Updated Meziantou.Analyzer to 2.0.18
- FF-1429 - Updated SonarAnalyzer.CSharp to 8.53.0.62665
- FF-1429 - Updated Microsoft.NET.Test.Sdk to 17.5.0
- FF-1429 - Updated Npgsql to 7.0.2
- FF-1429 - Updated Credfeto.Enumeration.Source.Generation to 1.0.6.18
- FF-1429 - Updated FunFair.Test.Common to 6.0.24.2725
- FF-1429 - Updated Microsoft.CodeAnalysis.CSharp.Workspaces to 4.5.0

## [0.0.1] - 2023-02-23
### Added
- Simple generation for stored procedures and user defined functions
### Changed
- FF-1429 - Updated FunFair.Test.Common to 6.0.20.2640
- FF-1429 - Updated NSubstitute to 5.0.0
- FF-3881 - Updated DotNet SDK to 7.0.200

## [0.0.0] - Project created