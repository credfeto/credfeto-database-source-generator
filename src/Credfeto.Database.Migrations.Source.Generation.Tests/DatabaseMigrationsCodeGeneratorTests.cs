using System;
using System.Collections.Generic;
using System.Linq;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Credfeto.Database.Migrations.Source.Generation.Tests;

public sealed class DatabaseMigrationsCodeGeneratorTests : TestBase
{
    private const string Source = """
        using Credfeto.Database.Migrations;

        [DatabaseMigrations]
        internal static partial class AppMigrations;
        """;

    [Fact]
    public void MigrationsAreOrderedById()
    {
        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(
            Source,
            ("0003_add_index.sql", "CREATE INDEX ix_accounts ON accounts (email);"),
            ("0001_create_accounts.sql", "CREATE TABLE accounts (id INT);"),
            ("0002_add_column.sql", "ALTER TABLE accounts ADD email TEXT;")
        );

        string generated = SoleGeneratedSource(result);

        int index1 = generated.IndexOf("create_accounts", StringComparison.Ordinal);
        int index2 = generated.IndexOf("add_column", StringComparison.Ordinal);
        int index3 = generated.IndexOf("add_index", StringComparison.Ordinal);

        Assert.True(index1 < index2, userMessage: "Expected migration 1 before migration 2");
        Assert.True(index2 < index3, userMessage: "Expected migration 2 before migration 3");
    }

    [Fact]
    public void NonMatchingFileNamesAreIgnored()
    {
        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(
            Source,
            ("0001_create_accounts.sql", "CREATE TABLE accounts (id INT);"),
            ("readme.sql", "-- not a migration"),
            ("notes.txt", "0002_not_sql")
        );

        string generated = SoleGeneratedSource(result);

        Assert.Contains("create_accounts", generated, StringComparison.Ordinal);
        Assert.DoesNotContain("not a migration", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void UpperCaseSqlExtensionIsRecognizedAsMigration()
    {
        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(
            Source,
            ("0001_create_accounts.SQL", "CREATE TABLE accounts (id INT);")
        );

        string generated = SoleGeneratedSource(result);

        Assert.Contains("create_accounts", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void GapsInNumberingAreAllowed()
    {
        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(
            Source,
            ("0001_create_accounts.sql", "CREATE TABLE accounts (id INT);"),
            ("0005_add_index.sql", "CREATE INDEX ix_accounts ON accounts (id);")
        );

        string generated = SoleGeneratedSource(result);

        Assert.Contains("create_accounts", generated, StringComparison.Ordinal);
        Assert.Contains("add_index", generated, StringComparison.Ordinal);
    }

    [Fact]
    public void DuplicateIdsProduceDiagnosticAndNoSource()
    {
        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(
            Source,
            ("0001_create_accounts.sql", "CREATE TABLE accounts (id INT);"),
            ("0001_create_orders.sql", "CREATE TABLE orders (id INT);")
        );

        GeneratorRunResult generatorResult = result.Results[0];

        Assert.Contains(generatorResult.Diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "CDMG002"));
        Assert.Empty(generatorResult.GeneratedSources);
    }

    [Fact]
    public void NonPartialClassProducesDiagnostic()
    {
        const string source = """
            using Credfeto.Database.Migrations;

            [DatabaseMigrations]
            internal static class AppMigrations;
            """;

        GeneratorDriverRunResult result = CompilationHelpers.RunGenerator(
            source,
            ("0001_create_accounts.sql", "CREATE TABLE accounts (id INT);")
        );

        GeneratorRunResult generatorResult = result.Results[0];

        Assert.Contains(generatorResult.Diagnostics, d => StringComparer.Ordinal.Equals(d.Id, "CDMG001"));
        Assert.Empty(generatorResult.GeneratedSources);
    }

    private static string SoleGeneratedSource(GeneratorDriverRunResult result)
    {
        GeneratorRunResult generatorResult = result.Results[0];

        IReadOnlyList<Diagnostic> allDiagnostics = [.. generatorResult.Diagnostics];
        string diagnosticsMessage = string.Join(
            separator: "; ",
            allDiagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")
        );

        Assert.True(
            generatorResult.GeneratedSources.Length == 1,
            userMessage: $"Expected exactly 1 generated source. Diagnostics: [{diagnosticsMessage}]"
        );

        return generatorResult.GeneratedSources[0].SourceText.ToString();
    }
}
