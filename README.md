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

## Usage

### 1. Define a result model

Create a record (or class) whose property names match the database column names. Use `[SqlFieldMap<TMapper, TType>]`
on any property that requires custom type conversion:

```csharp
using Credfeto.Database.Interfaces;

public sealed record Accounts(
    int Id,
    string Name,
    [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress Address,
    DateTime LastModified
);
```

### 2. Implement a mapper (for custom types)

Implement `IMapper<T>` to control how a custom type is read from / written to the database:

```csharp
using System.Data;
using System.Data.Common;
using Credfeto.Database.Interfaces;

internal sealed class AccountAddressMapper : IMapper<AccountAddress>
{
    public static AccountAddress MapFromDb(object value) =>
        new() { Value = (string)value };

    public static void MapToDb(AccountAddress value, DbParameter parameter)
    {
        parameter.Value = value.Value;
        parameter.DbType = DbType.String;
        parameter.Size = 100;
    }
}
```

### 3. Declare the database wrapper

Create an `internal static partial` class and annotate each method with `[SqlObjectMap]`. The generator fills
in the implementation at compile time:

```csharp
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;

internal static partial class DatabaseWrapper
{
    // Table-valued function — returns multiple rows
    [SqlObjectMap(name: "schema.account_getall",
        sqlObjectType: SqlObjectType.TABLE_FUNCTION,
        sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<IReadOnlyList<Accounts>> GetAllAsync(
        DbConnection connection,
        [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress address,
        CancellationToken cancellationToken);

    // Stored procedure — no return value
    [SqlObjectMap(name: "schema.account_insert",
        sqlObjectType: SqlObjectType.STORED_PROCEDURE,
        sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask InsertAsync(
        DbConnection connection,
        string name,
        [SqlFieldMap<AccountAddressMapper, AccountAddress>] AccountAddress address,
        CancellationToken cancellationToken);

    // Scalar function — returns a single value
    [SqlObjectMap(name: "schema.get_meaning_of_life",
        sqlObjectType: SqlObjectType.SCALAR_FUNCTION,
        sqlDialect: SqlDialect.GENERIC)]
    public static partial ValueTask<int> GetMeaningOfLifeAsync(
        DbConnection connection,
        CancellationToken cancellationToken);
}
```

Supported `SqlObjectType` values: `TABLE_FUNCTION`, `STORED_PROCEDURE`, `SCALAR_FUNCTION`.
Supported `SqlDialect` values: `GENERIC`, `MICROSOFT_SQL_SERVER`.

### 4. What the generator produces

For each annotated method the generator emits a complete implementation in a `*.generated.cs` file. For example,
`GetAllAsync` above produces code that:

- builds a `DbCommand` with the appropriate SQL (`SELECT … FROM schema.account_getall(@address)`),
- maps each input parameter (calling `AccountAddressMapper.MapToDb` for custom types),
- executes the reader and maps each result row back to an `Accounts` record (calling
  `AccountAddressMapper.MapFromDb` for custom types),
- returns the results as `IReadOnlyList<Accounts>`.

You never write this boilerplate by hand — the generator keeps it in sync with your method signatures
automatically.

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

## Contributors

<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore-start -->
<!-- markdownlint-disable -->

<!-- markdownlint-restore -->
<!-- prettier-ignore-end -->

<!-- ALL-CONTRIBUTORS-LIST:END -->
