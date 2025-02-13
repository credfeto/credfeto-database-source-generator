namespace Credfeto.Database.Source.Generation.Models;

internal sealed class SqlObject
{
    public SqlObject(string name, SqlObjectType sqlObjectType, SqlDialect sqlDialect)
    {
        this.Name = name;
        this.SqlObjectType = sqlObjectType;
        this.SqlDialect = sqlDialect;
    }

    public SqlObjectType SqlObjectType { get; }

    public SqlDialect SqlDialect { get; }

    public string Name { get; }
}
