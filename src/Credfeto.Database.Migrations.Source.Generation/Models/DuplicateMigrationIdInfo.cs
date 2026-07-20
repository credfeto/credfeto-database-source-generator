using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("Id = {Id}")]
internal readonly record struct DuplicateMigrationIdInfo(long Id, Location Location);
