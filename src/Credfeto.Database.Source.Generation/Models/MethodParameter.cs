using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodParameter
{
    public MethodParameter(string name, ISymbol type, MethodParameterUsage usage, MapperInfo? mapperInfo)
    {
        this.Name = name;
        this.Type = type;
        this.Usage = usage;
        this.MapperInfo = mapperInfo;
    }

    public string Name { get; }

    public ISymbol Type { get; }

    public MethodParameterUsage Usage { get; }

    public MapperInfo? MapperInfo { get; }
}