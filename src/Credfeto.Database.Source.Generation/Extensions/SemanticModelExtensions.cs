using System.Threading;
using Microsoft.CodeAnalysis;

namespace Credfeto.Database.Source.Generation.Extensions;

internal static class SemanticModelExtensions
{
    public static ISymbol? GetSymbol(this SemanticModel semanticModel, SyntaxNode node, CancellationToken cancellationToken)
    {
        return semanticModel.GetDeclaredSymbol(declaration: node, cancellationToken: cancellationToken) ?? semanticModel
                                                                                                           .GetSymbolInfo(node: node, cancellationToken: cancellationToken)
                                                                                                           .Symbol;
    }
}