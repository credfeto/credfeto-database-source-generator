using System.Diagnostics;

namespace Credfeto.Database.Migrations;

[DebuggerDisplay("TableName = {TableName}")]
public sealed record MigrationRunnerOptions(string TableName = "__migrations");
