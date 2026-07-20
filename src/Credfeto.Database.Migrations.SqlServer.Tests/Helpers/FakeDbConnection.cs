using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Migrations.SqlServer.Tests.Helpers;

internal sealed class FakeDbConnection : DbConnection
{
    private readonly List<FakeDbTransaction> _transactions = [];

    public FakeDbConnection(IReadOnlyList<long>? appliedIds = null)
    {
        this.AppliedIds = appliedIds ?? [];
    }

    public List<string> ExecutedSql { get; } = [];

    public IReadOnlyList<FakeDbTransaction> Transactions => this._transactions;

    public IReadOnlyList<long> AppliedIds { get; }

    [AllowNull]
    public override string ConnectionString { get; set; } = string.Empty;

    public override string Database => string.Empty;

    public override string DataSource => string.Empty;

    public override string ServerVersion => string.Empty;

    public override ConnectionState State => ConnectionState.Open;

    public void RecordExecution(string sql)
    {
        this.ExecutedSql.Add(sql);
    }

    public override void ChangeDatabase(string databaseName) { }

    public override void Close() { }

    public override void Open() { }

    protected override DbCommand CreateDbCommand()
    {
        return new FakeDbCommand(this);
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        FakeDbTransaction transaction = new(this);
        this._transactions.Add(transaction);

        return transaction;
    }
}
