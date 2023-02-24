using System;

namespace Credfeto.Database.Interfaces;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public sealed class SqlFieldMapAttribute<TM, TD> : Attribute
#if NET7_0_OR_GREATER
    where TM : IMapper<TD>
#endif
{
}