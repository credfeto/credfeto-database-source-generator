using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database.Migrations.Pgsql;

[SuppressMessage(
    category: "Microsoft.Security",
    checkId: "CA2100:Review SQL queries for security vulnerabilities",
    Justification = "Table name is validated as a safe identifier before being embedded in the SQL text"
)]
[SuppressMessage(
    category: "SonarAnalyzer.CSharp",
    checkId: "S2077:Use a parameterized query instead of string formatting",
    Justification = "Table name is validated as a safe identifier before being embedded in the SQL text"
)]
public sealed class PgsqlMigrationTracker : MigrationTrackerBase
{
    public PgsqlMigrationTracker(MigrationRunnerOptions? options = null)
        : base(options) { }

    public override async ValueTask EnsureCreatedAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);

        DbCommand command = connection.CreateCommand();

        await using (command)
        {
            command.CommandText = $"""
                CREATE TABLE IF NOT EXISTS "{this.TableName}" (
                    id BIGINT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    applied_at_utc TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT (now() AT TIME ZONE 'utc')
                )
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    protected override string BuildSelectAppliedIdsSql()
    {
        return $"""SELECT id FROM "{this.TableName}" """;
    }

    protected override string BuildInsertAppliedSql()
    {
        return $"""INSERT INTO "{this.TableName}" (id, name) VALUES (@id, @name)""";
    }
}
