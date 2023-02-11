using System;
using System.Diagnostics;

namespace Stuff;

[DebuggerDisplay("Name = {Name}, SqlObjectType = {SqlObjectType}")]
[AttributeUsage(AttributeTargets.Method)]
internal sealed class SqlObjectMapAttribute : Attribute
{
    public SqlObjectMapAttribute(string name, SqlObjectType sqlObjectType)
    {
        this.Name = name;
        this.SqlObjectType = sqlObjectType;
    }

    public string Name { get; }

    public SqlObjectType SqlObjectType { get; }
}