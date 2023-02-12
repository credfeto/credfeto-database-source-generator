using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MapperInfo
{
    public MapperInfo(ISymbol mapperSymbol, ISymbol mappedSymbol)
    {
        this.MapperSymbol = mapperSymbol;
        this.MappedSymbol = mappedSymbol;
    }

    public ISymbol MapperSymbol { get; }

    public ISymbol MappedSymbol { get; }
}