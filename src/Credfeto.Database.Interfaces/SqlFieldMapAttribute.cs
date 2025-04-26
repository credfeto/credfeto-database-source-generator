using System;
using System.Diagnostics.CodeAnalysis;

namespace Credfeto.Database.Interfaces;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
[SuppressMessage(
    category: "ReSharper",
    checkId: "UnusedTypeParameter",
    Justification = nameof(TMapper) + " is used in the source generator"
)]
public sealed class SqlFieldMapAttribute<TMapper, TDataType> : Attribute
    where TMapper : IMapper<TDataType>;
