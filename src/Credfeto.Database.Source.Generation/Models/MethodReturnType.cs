using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodReturnType
{
    public MethodReturnType(ISymbol returnType, ISymbol? collectionReturnType, ISymbol? elementReturnType, MapperInfo? mapperInfo)
    {
        this.ReturnType = returnType;
        this.CollectionReturnType = collectionReturnType;
        this.ElementReturnType = elementReturnType;
        this.MapperInfo = mapperInfo;
    }

    public ISymbol ReturnType { get; }

    public ISymbol? CollectionReturnType { get; }

    public ISymbol? ElementReturnType { get; }

    public MapperInfo? MapperInfo { get; }
}