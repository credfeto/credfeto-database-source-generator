# Changelog
All notable changes to this project will be documented in this file.

<!--
Please ADD ALL Changes to the UNRELEASED SECTION and not a specific release
-->

## [Unreleased]
### Added
### Fixed
### Changed
- Used ThisAssembly.AssemblyInfo to generate static version information
- Dependencies - Updated SonarAnalyzer.CSharp to 9.12.0.78982
- Dependencies - Updated TeamCity.VSTest.TestAdapter to 1.0.38
- Dependencies - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 1.3.1
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.8.0
- SDK - Updated DotNet SDK to 8.0.100
- Dependencies - Updated Microsoft.Extensions to 8.0.0
- Dependencies - Updated FunFair.CodeAnalysis to 7.0.4.198
- Dependencies - Updated Meziantou.Analyzer to 2.0.110
- Dependencies - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.8.14
- Dependencies - Updated Polly to 8.2.0
- Dependencies - Updated Credfeto.Enumeration.Source.Generation to 1.1.1.168
- Dependencies - Updated xunit.analyzers to 1.6.0
- Dependencies - Updated xunit to 2.6.2
- Dependencies - Updated xunit.runner.visualstudio to 2.5.4
- Dependencies - Updated Npgsql to 8.0.0
- Dependencies - Updated Microsoft.CodeAnalysis.CSharp to 4.8.0
- Dependencies - Updated FluentValidation to 11.8.1
- Dependencies - Updated Roslynator.Analyzers to 4.6.4
- Dependencies - Updated Philips.CodeAnalysis.MaintainabilityAnalyzers to 1.4.0
- Dependencies - Updated FunFair.Test.Common to 6.1.21.247
### Removed
### Deployment Changes

<!--
Releases that have at least been deployed to staging, BUT NOT necessarily released to live.  Changes should be moved from [Unreleased] into here as they are merged into the appropriate release branch
-->
## [1.2.1] - 2023-09-14
### Changed
- Dependencies - .NET 8 Preview 3
- Dependencies - Updated coverlet to 6.0.0
- Dependencies - Updated FunFair.Test.Common to 6.1.1.49
- Dependencies - Updated Microsoft.Extensions to 7.0.1
- Dependencies - Updated Polly to 7.2.4
- Dependencies - Updated Microsoft.CodeAnalysis.CSharp to 4.7.0
- Dependencies - Updated Microsoft.VisualStudio.Threading.Analyzers to 17.7.30
- Dependencies - Updated xunit.analyzers to 1.2.0
- Dependencies - Updated FluentValidation to 11.7.1
- Dependencies - Updated xunit to 2.5.0
- Dependencies - Updated xunit.runner.visualstudio to 2.5.0
- Dependencies - Updated TeamCity.VSTest.TestAdapter to 1.0.37
- Dependencies - Updated Roslynator.Analyzers to 4.5.0
- Dependencies - Updated FunFair.CodeAnalysis to 7.0.2.121
- Dependencies - Updated Microsoft.NET.Test.Sdk to 17.7.2
- Dependencies - Updated SonarAnalyzer.CSharp to 9.9.0.77355
- Dependencies - Updated Meziantou.Analyzer to 2.0.84
- Dependencies - Updated NSubstitute to 5.1.0
- Dependencies - Updated Credfeto.Enumeration.Source.Generation to 1.1.0.138
- SDK - Updated DotNet SDK to 8.0.100-rc.1.23455.8

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