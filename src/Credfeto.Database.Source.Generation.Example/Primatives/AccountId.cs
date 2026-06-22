using System.Diagnostics;

namespace Credfeto.Database.Source.Generation.Example.Primatives;

[DebuggerDisplay("{Value}")]
public readonly record struct AccountId(long Value);
