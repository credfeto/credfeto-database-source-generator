# credfeto-database-source-generator

Database source generator

## Using

Add a reference to the ``Credfeto.Enumeration.Source.Generation`` package in each project you need the code generation
to run.

```xml
<ItemGroup>
  <PackageReference 
            Include="Credfeto.Database.Source.Generation" 
            Version="1.1.1.1" 
            PrivateAssets="All" 
            ExcludeAssets="runtime" />
</ItemGroup>
```

Reference the following package in the project that contains the attributes and interfaces that are used by the
generator.

```xml
<ItemGroup>
    <PackageReference
            Include="Credfeto.Database.Interfaces"
            Version="1.1.1.1"
            PrivateAssets="All" 
            ExcludeAssets="runtime" />
</ItemGroup>
```

## Viewing Compiler Generated files

Add the following to the csproj file:

```xml
  <PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>
  <ItemGroup>
    <!-- Don't include the output from a previous source generator execution into future runs; the */** trick here ensures that there's
    at least one subdirectory, which is our key that it's coming from a source generator as opposed to something that is coming from
    some other tool. -->
    <Compile Remove="$(CompilerGeneratedFilesOutputPath)/*/**/*.cs" />
  </ItemGroup>
```

## Build Status

| Branch  | Status                                                                                                                                                                                                                                                                    |
|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| main    | [![Build: Pre-Release](https://github.com/credfeto/credfeto-database-source-generator/actions/workflows/build-and-publish-pre-release.yml/badge.svg)](https://github.com/credfeto/credfeto-database-source-generator/actions/workflows/build-and-publish-pre-release.yml) |
| release | [![Build: Release](https://github.com/credfeto/credfeto-database-source-generator/actions/workflows/build-and-publish-release.yml/badge.svg)](https://github.com/credfeto/credfeto-database-source-generator/actions/workflows/build-and-publish-release.yml)             |

## Changelog

View [changelog](CHANGELOG.md)
