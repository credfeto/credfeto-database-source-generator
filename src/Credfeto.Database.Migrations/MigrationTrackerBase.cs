using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database.Migrations;

[SuppressMessage(
    category: "Microsoft.Security",
    checkId: "CA2100:Review SQL queries for security vulnerabilities",
    Justification = "Table name is validated as a safe identifier before being embedded in the SQL text by the derived tracker's Build*Sql overrides"
)]
[SuppressMessage(
    category: "SonarAnalyzer.CSharp",
    checkId: "S2077:Use a parameterized query instead of string formatting",
    Justification = "Table name is validated as a safe identifier before being embedded in the SQL text by the derived tracker's Build*Sql overrides"
)]
public abstract partial class MigrationTrackerBase : IMigrationTracker
{
    protected MigrationTrackerBase(MigrationRunnerOptions? options)
    {
        this.TableName = ValidateTableName((options ?? new MigrationRunnerOptions()).TableName);
    }

    protected string TableName { get; }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TableNamePattern();

    private static string ValidateTableName(string tableName)
    {
        if (!TableNamePattern().IsMatch(tableName))
        {
            throw new ArgumentException(
                $"Migration table name '{tableName}' is not a valid identifier.",
                nameof(tableName)
            );
        }

        return tableName;
    }

    public abstract ValueTask EnsureCreatedAsync(DbConnection connection, CancellationToken cancellationToken);

    protected abstract string BuildSelectAppliedIdsSql();

    protected abstract string BuildInsertAppliedSql();

    public async ValueTask<IReadOnlySet<long>> GetAppliedMigrationIdsAsync(
        DbConnection connection,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(connection);

        HashSet<long> applied = [];

        DbCommand command = connection.CreateCommand();

        await using (command)
        {
            command.CommandText = this.BuildSelectAppliedIdsSql();

            DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

            await using (reader)
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    applied.Add(reader.GetInt64(0));
                }
            }
        }

        return applied;
    }

    public async ValueTask RecordAppliedAsync(
        DbConnection connection,
        DbTransaction transaction,
        IMigration migration,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(migration);

        DbCommand command = connection.CreateCommand();

        await using (command)
        {
            command.Transaction = transaction;
            command.CommandText = this.BuildInsertAppliedSql();

            DbParameter idParameter = command.CreateParameter();
            idParameter.ParameterName = "id";
            idParameter.Value = migration.Id;
            command.Parameters.Add(idParameter);

            DbParameter nameParameter = command.CreateParameter();
            nameParameter.ParameterName = "name";
            nameParameter.Value = migration.Name;
            command.Parameters.Add(nameParameter);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
