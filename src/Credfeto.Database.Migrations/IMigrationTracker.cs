using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database.Migrations;

public interface IMigrationTracker
{
    ValueTask EnsureCreatedAsync(DbConnection connection, CancellationToken cancellationToken);

    ValueTask<IReadOnlySet<long>> GetAppliedMigrationIdsAsync(
        DbConnection connection,
        CancellationToken cancellationToken
    );

    ValueTask RecordAppliedAsync(
        DbConnection connection,
        DbTransaction transaction,
        IMigration migration,
        CancellationToken cancellationToken
    );
}
