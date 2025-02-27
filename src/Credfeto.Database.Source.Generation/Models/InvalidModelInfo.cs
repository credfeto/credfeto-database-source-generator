using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

[DebuggerDisplay("{Location} {Message}")]
internal readonly record struct InvalidModelInfo
{
    public InvalidModelInfo(Location location, string message)
    {
        this.Location = location;
        this.Message = message;
    }

    public string Message { get; }

    public Location Location { get; }
}
