using System.Diagnostics;

namespace Credfeto.Database.Source.Generation.Example.Primatives;

[DebuggerDisplay("{Value}")]
public sealed class AccountAddress
{
#if NET7_0_OR_GREATER
    public required string Value { get; init; }
#else
    public string Value { get; init; } = default!;
#endif
}