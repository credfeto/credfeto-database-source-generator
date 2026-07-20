using System.Diagnostics;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("Id = {Id}, Name = {Name}")]
internal readonly record struct MigrationSourceFile(long Id, string Name, string Sql);
