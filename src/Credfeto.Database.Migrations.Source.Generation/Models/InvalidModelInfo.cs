using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("Message = {Message}")]
internal readonly record struct InvalidModelInfo(string Message, Location Location);
