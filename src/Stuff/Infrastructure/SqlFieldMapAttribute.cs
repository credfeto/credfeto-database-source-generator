using System;

namespace Stuff;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SqlFieldMapAttribute<TM, TD> : Attribute
    where TM : IMapper<TD>
{
}