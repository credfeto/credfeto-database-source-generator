using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Migrations.Source.Generation.Models;

[DebuggerDisplay("Namespace = {Namespace}, ClassName = {ClassName}")]
internal readonly record struct AttributedClassInfo(
    string Namespace,
    string ClassName,
    string AccessModifier,
    bool IsPartial,
    Location Location
);
