using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Receivers;

public sealed class DatabaseSyntaxReceiver : ISyntaxContextReceiver
{
    private readonly List<MethodDeclarationSyntax> _methods = new();

    public IReadOnlyList<MethodDeclarationSyntax> Methods => this._methods;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return;
        }

        if (methodDeclarationSyntax.AttributeLists.Count == 0)
        {
            return;
        }

        this._methods.Add(item: methodDeclarationSyntax);
    }
}