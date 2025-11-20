using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

[DebuggerDisplay("{Code} {Location} {Message}")]
internal readonly record struct WarningModelInfo
{
    public WarningModelInfo(string code, Location location, string message)
    {
        this.Code = code;
        this.Location = location;
        this.Message = message;
    }

    public string Message { get; }

    public string Code { get; }

    public Location Location { get; }
}