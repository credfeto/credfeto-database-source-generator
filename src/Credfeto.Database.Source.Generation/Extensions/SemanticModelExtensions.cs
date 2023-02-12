using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Extensions;

internal static class SemanticModelExtensions
{
    public static ISymbol? GetSymbol(this SemanticModel semanticModel, SyntaxNode node)
    {
        ISymbol? symbol = semanticModel.GetDeclaredSymbol(node) ?? semanticModel.GetSymbolInfo(node)
                                                                                .Symbol;

        return symbol;
    }
}