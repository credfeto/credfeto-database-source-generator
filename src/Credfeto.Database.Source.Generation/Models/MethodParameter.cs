using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodParameter
{
    public MethodParameter(string name, ISymbol type, MethodParameterUsage usage, bool nullable, MapperInfo? mapperInfo)
    {
        this.Name = name;
        this.Type = type;
        this.Usage = usage;
        this.Nullable = nullable;
        this.MapperInfo = mapperInfo;
    }

    public string Name { get; }

    public ISymbol Type { get; }

    public MethodParameterUsage Usage { get; }

    public bool Nullable { get; }

    public MapperInfo? MapperInfo { get; }
}
