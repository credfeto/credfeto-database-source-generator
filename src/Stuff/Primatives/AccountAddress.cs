using System.Diagnostics;

namespace Stuff;

[DebuggerDisplay("{Value}")]
public sealed class AccountAddress
{
    public required string Value { get; init; }
}