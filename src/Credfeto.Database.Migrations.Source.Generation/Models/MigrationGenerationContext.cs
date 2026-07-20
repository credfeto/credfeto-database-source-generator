using System.Collections.Immutable;
using System.Diagnostics;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("ClassGeneration = {ClassGeneration}")]
internal readonly record struct MigrationGenerationContext(
    MigrationClassGeneration? ClassGeneration,
    InvalidModelInfo? InvalidModel,
    ImmutableArray<DuplicateMigrationIdInfo> Duplicates
);
