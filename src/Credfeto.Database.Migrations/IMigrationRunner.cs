using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Credfeto.Database.Migrations;

public interface IMigrationRunner
{
    ValueTask MigrateAsync(IReadOnlyList<IMigration> migrations, CancellationToken cancellationToken);
}
