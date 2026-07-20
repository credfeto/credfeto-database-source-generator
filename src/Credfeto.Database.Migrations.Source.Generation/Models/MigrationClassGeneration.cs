using System.Collections.Immutable;
using System.Diagnostics;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("Namespace = {Namespace}, ClassName = {ClassName}")]
internal readonly record struct MigrationClassGeneration(
    string Namespace,
    string ClassName,
    string AccessModifier,
    ImmutableArray<MigrationSourceFile> Migrations
);
