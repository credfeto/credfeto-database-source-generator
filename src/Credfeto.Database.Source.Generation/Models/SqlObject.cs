namespace Credfeto.Database.Source.Generation.Models;

internal sealed class SqlObject
{
    public SqlObject(string name, SqlObjectType sqlObjectType)
    {
        this.Name = name;
        this.SqlObjectType = sqlObjectType;
    }

    public SqlObjectType SqlObjectType { get; }

    public string Name { get; }
}