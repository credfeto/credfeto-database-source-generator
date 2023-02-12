using System;

namespace Credfeto.Database.Interfaces;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public sealed class SqlFieldMapAttribute<TM, TD> : Attribute
    where TM : IMapper<TD>
{
}