using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Credfeto.Database.Migrations.Source.Generation.Models;
using Microsoft.CodeAnalysis.Text;

namespace Credfeto.Database.Migrations.Source.Generation.Extensions;

internal static class MigrationFileNameExtensions
{
    private static readonly Regex FileNamePattern = new(
        pattern: @"^(?<id>\d+)_(?<name>[A-Za-z0-9_]+)\.sql$",
        options: RegexOptions.CultureInvariant,
        matchTimeout: TimeSpan.FromSeconds(1)
    );

    public static bool TryParse(string fileName, SourceText sql, out MigrationSourceFile migration)
    {
        Match match = FileNamePattern.Match(fileName);

        if (
            !match.Success
            || !long.TryParse(
                match.Groups["id"].Value,
                style: NumberStyles.None,
                provider: CultureInfo.InvariantCulture,
                result: out long id
            )
        )
        {
            migration = default;

            return false;
        }

        string name = match.Groups["name"].Value;

        migration = new(id, name, sql.ToString());

        return true;
    }
}
