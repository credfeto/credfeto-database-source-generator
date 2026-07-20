using System.Diagnostics;

namespace Credfeto.Database.Migrations;

[DebuggerDisplay("Id = {Id}, Name = {Name}")]
public sealed record Migration(long Id, string Name, string Sql) : IMigration;
