using System;
using System.Diagnostics;

namespace Credfeto.Database.Interfaces;

[DebuggerDisplay("Name = {Name}, SqlObjectType = {SqlObjectType} Dialect: {SqlDialect}")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class SqlObjectMapAttribute : Attribute
{
    public SqlObjectMapAttribute(string name, SqlObjectType sqlObjectType, SqlDialect sqlDialect)
    {
        this.Name = name;
        this.SqlObjectType = sqlObjectType;
        this.SqlDialect = sqlDialect;
    }

    public string Name { get; }

    public SqlObjectType SqlObjectType { get; }

    public SqlDialect SqlDialect { get; }
}