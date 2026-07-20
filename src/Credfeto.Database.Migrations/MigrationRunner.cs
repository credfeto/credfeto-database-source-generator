using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database.Migrations;

public sealed class MigrationRunner : IMigrationRunner
{
    private readonly IDatabase _database;
    private readonly IMigrationTracker _tracker;

    public MigrationRunner(IDatabase database, IMigrationTracker tracker)
    {
        this._database = database ?? throw new ArgumentNullException(nameof(database));
        this._tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
    }

    public ValueTask MigrateAsync(IReadOnlyList<IMigration> migrations, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(migrations);

        IReadOnlyList<IMigration> ordered = OrderAndValidate(migrations);

        return this._database.ExecuteAsync(
            action: (connection, ct) =>
                this.ApplyMigrationsAsync(connection: connection, migrations: ordered, cancellationToken: ct),
            cancellationToken: cancellationToken
        );
    }

    private static IReadOnlyList<IMigration> OrderAndValidate(IReadOnlyList<IMigration> migrations)
    {
        IMigration[] ordered = [.. migrations.OrderBy(keySelector: static migration => migration.Id)];

        for (int index = 1; index < ordered.Length; ++index)
        {
            if (ordered[index].Id == ordered[index - 1].Id)
            {
                throw new InvalidOperationException(
                    $"Duplicate migration Id {ordered[index].Id.ToString(provider: CultureInfo.InvariantCulture)}."
                );
            }
        }

        return ordered;
    }

    private async ValueTask ApplyMigrationsAsync(
        DbConnection connection,
        IReadOnlyList<IMigration> migrations,
        CancellationToken cancellationToken
    )
    {
        await this._tracker.EnsureCreatedAsync(connection: connection, cancellationToken: cancellationToken);

        IReadOnlySet<long> applied = await this._tracker.GetAppliedMigrationIdsAsync(
            connection: connection,
            cancellationToken: cancellationToken
        );

        foreach (IMigration migration in migrations)
        {
            if (applied.Contains(migration.Id))
            {
                continue;
            }

            await this.ApplyMigrationAsync(
                connection: connection,
                migration: migration,
                cancellationToken: cancellationToken
            );
        }
    }

    [SuppressMessage(
        category: "Microsoft.Security",
        checkId: "CA2100:Review SQL queries for security vulnerabilities",
        Justification = "Migration SQL is developer-authored and compiled in, not external input"
    )]
    private async ValueTask ApplyMigrationAsync(
        DbConnection connection,
        IMigration migration,
        CancellationToken cancellationToken
    )
    {
        DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (transaction)
        {
            try
            {
                DbCommand command = connection.CreateCommand();

                await using (command)
                {
                    command.Transaction = transaction;
                    command.CommandText = migration.Sql;

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                await this._tracker.RecordAppliedAsync(
                    connection: connection,
                    transaction: transaction,
                    migration: migration,
                    cancellationToken: cancellationToken
                );

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        }
    }
}
