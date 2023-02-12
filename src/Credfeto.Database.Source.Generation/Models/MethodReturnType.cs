using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Models;

internal sealed class MethodReturnType
{
    public MethodReturnType(ISymbol returnType, ISymbol? collectionReturnType, ISymbol? elementReturnType)
    {
        this.ReturnType = returnType;
        this.CollectionReturnType = collectionReturnType;
        this.ElementReturnType = elementReturnType;
    }

    public ISymbol ReturnType { get; }

    public ISymbol? CollectionReturnType { get; }

    public ISymbol? ElementReturnType { get; }
}