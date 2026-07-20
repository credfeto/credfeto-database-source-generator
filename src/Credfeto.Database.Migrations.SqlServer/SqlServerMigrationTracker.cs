using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database.Migrations.SqlServer;

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
public sealed class SqlServerMigrationTracker : MigrationTrackerBase
{
    public SqlServerMigrationTracker(MigrationRunnerOptions? options = null)
        : base(options) { }

    public override async ValueTask EnsureCreatedAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);

        DbCommand command = connection.CreateCommand();

        await using (command)
        {
            command.CommandText = $"""
                IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = @tableName)
                BEGIN
                    CREATE TABLE [{this.TableName}] (
                        id BIGINT NOT NULL PRIMARY KEY,
                        name NVARCHAR(400) NOT NULL,
                        applied_at_utc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                    )
                END
                """;

            DbParameter tableNameParameter = command.CreateParameter();
            tableNameParameter.ParameterName = "tableName";
            tableNameParameter.Value = this.TableName;
            command.Parameters.Add(tableNameParameter);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    protected override string BuildSelectAppliedIdsSql()
    {
        return $"SELECT id FROM [{this.TableName}]";
    }

    protected override string BuildInsertAppliedSql()
    {
        return $"INSERT INTO [{this.TableName}] (id, name) VALUES (@id, @name)";
    }
}
