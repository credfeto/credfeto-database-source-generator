using System.Globalization;
using Credfeto.Database.Migrations.Source.Generation.Builders;
using Credfeto.Database.Migrations.Source.Generation.Models;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Migrations.Source.Generation;

internal static class MigrationSourceCodeGenerator
{
    public static void Generate(in SourceProductionContext context, in MigrationClassGeneration generation)
    {
        CodeBuilder builder = new CodeBuilder()
            .AppendFileHeader()
            .AppendLine("#nullable enable")
            .AppendBlankLine()
            .AppendLine("using System.CodeDom.Compiler;")
            .AppendLine("using System.Collections.Generic;")
            .AppendLine("using Credfeto.Database.Migrations;")
            .AppendBlankLine();

        if (generation.Namespace.Length != 0)
        {
            builder.AppendLine($"namespace {generation.Namespace};").AppendBlankLine();
        }

        builder.AppendLine(
            $"[GeneratedCode(tool: \"{VersionInformation.Product}\", version: \"{VersionInformation.Version}\")]"
        );

        using (builder.StartBlock($"{generation.AccessModifier} partial class {generation.ClassName}"))
        {
            using (
                builder.StartBlock(
                    "public static IReadOnlyList<IMigration> Migrations { get; } = new IMigration[]",
                    start: "{",
                    end: "};"
                )
            )
            {
                foreach (MigrationSourceFile migration in generation.Migrations)
                {
                    builder.AppendLine(
                        $"new Migration(Id: {migration.Id.ToString(CultureInfo.InvariantCulture)}, Name: {EscapeString(migration.Name)}, Sql: {EscapeString(migration.Sql)}),"
                    );
                }
            }
        }

        string hintNamePrefix =
            generation.Namespace.Length == 0 ? generation.ClassName : $"{generation.Namespace}.{generation.ClassName}";

        context.AddSource(hintName: $"{hintNamePrefix}.Migrations.g.cs", sourceText: builder.Text);
    }

    private static string EscapeString(string value)
    {
        return "\""
            + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\r\\n").Replace("\n", "\\n")
            + "\"";
    }
}
