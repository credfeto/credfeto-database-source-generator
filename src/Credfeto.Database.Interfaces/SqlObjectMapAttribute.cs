using System;
using System.Diagnostics;

namespace Credfeto.Database.Interfaces;

[DebuggerDisplay("Name = {Name}, SqlObjectType = {SqlObjectType}")]
[AttributeUsage(AttributeTargets.Method)]
public sealed class SqlObjectMapAttribute : Attribute
{
    public SqlObjectMapAttribute(string name, SqlObjectType sqlObjectType)
    {
        this.Name = name;
        this.SqlObjectType = sqlObjectType;
    }

    public string Name { get; }

    public SqlObjectType SqlObjectType { get; }
}