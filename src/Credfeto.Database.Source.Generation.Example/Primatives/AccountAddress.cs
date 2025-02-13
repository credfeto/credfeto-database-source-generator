using System.Diagnostics;

namespace Credfeto.Database.Source.Generation.Example.Primatives;

[DebuggerDisplay("{Value}")]
public sealed class AccountAddress
{
    public required string Value { get; init; }
}
