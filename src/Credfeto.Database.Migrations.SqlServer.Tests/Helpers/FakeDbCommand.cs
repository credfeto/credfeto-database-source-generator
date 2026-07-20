using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Migrations.SqlServer.Tests.Helpers;

internal sealed class FakeDbCommand : DbCommand
{
    private readonly FakeDbConnection _connection;
    private readonly FakeDbParameterCollection _parameters = [];

    public FakeDbCommand(FakeDbConnection connection)
    {
        this._connection = connection;
    }

    [AllowNull]
    public override string CommandText { get; set; } = string.Empty;

    public override int CommandTimeout { get; set; }

    public override CommandType CommandType { get; set; } = CommandType.Text;

    public override bool DesignTimeVisible { get; set; }

    public override UpdateRowSource UpdatedRowSource { get; set; }

    protected override DbConnection? DbConnection { get; set; }

    protected override DbParameterCollection DbParameterCollection => this._parameters;

    protected override DbTransaction? DbTransaction { get; set; }

    public override void Cancel() { }

    public override int ExecuteNonQuery()
    {
        this._connection.RecordExecution(this.CommandText);

        return 1;
    }

    public override object? ExecuteScalar()
    {
        this._connection.RecordExecution(this.CommandText);

        return null;
    }

    public override void Prepare() { }

    protected override DbParameter CreateDbParameter()
    {
        return new FakeDbParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        this._connection.RecordExecution(this.CommandText);

        return new FakeDbDataReader(this._connection.AppliedIds);
    }
}
