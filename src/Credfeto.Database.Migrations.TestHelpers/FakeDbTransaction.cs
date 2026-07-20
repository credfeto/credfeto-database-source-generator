using System.Data;
using System.Data.Common;

namespace Credfeto.Database.Migrations.TestHelpers;

public sealed class FakeDbTransaction : DbTransaction
{
    private readonly FakeDbConnection _connection;

    public FakeDbTransaction(FakeDbConnection connection)
    {
        this._connection = connection;
    }

    public bool Committed { get; private set; }

    public bool RolledBack { get; private set; }

    public override IsolationLevel IsolationLevel => IsolationLevel.Unspecified;

    protected override DbConnection DbConnection => this._connection;

    public override void Commit()
    {
        this.Committed = true;
    }

    public override void Rollback()
    {
        this.RolledBack = true;
    }
}
