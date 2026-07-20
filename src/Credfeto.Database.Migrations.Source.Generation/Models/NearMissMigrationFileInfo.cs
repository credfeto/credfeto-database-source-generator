using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("FileName = {FileName}")]
internal readonly record struct NearMissMigrationFileInfo(string FileName, Location Location);
