using System;

namespace Credfeto.Database.Interfaces;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SqlFieldMapAttribute<TM, TD> : Attribute
    where TM : IMapper<TD>
{
}