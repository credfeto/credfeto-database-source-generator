using System.Diagnostics;

namespace Stuff.Primatives;

[DebuggerDisplay("{Value}")]
public sealed class AccountAddress
{
    public required string Value { get; init; }
}