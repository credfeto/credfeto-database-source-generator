using System.Collections.Generic;
using System.Linq;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Receivers;

internal sealed class DatabaseSyntaxReceiver : ISyntaxContextReceiver
{
    private readonly List<MethodGeneration> _methods = new();

    public IReadOnlyList<MethodGeneration> Methods => this._methods;

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

        if (!methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        ClassDeclarationSyntax? classDeclarationSyntax = methodDeclarationSyntax.Ancestors()
                                                                                .OfType<ClassDeclarationSyntax>()
                                                                                .FirstOrDefault();

        if (classDeclarationSyntax is null)
        {
            return;
        }

        if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        ClassInfo containingContext = GetClass(context: context, classDeclarationSyntax: classDeclarationSyntax);

        this._methods.Add(item: new(containingContext: containingContext,
                                    methodDeclarationSyntax.GetAccessType(),
                                    methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword),
                                    method: methodDeclarationSyntax));
    }

    private static ClassInfo GetClass(in GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax)!;
        ClassInfo containingContext = new(symbol.ContainingNamespace.ToDisplayString(),
                                          name: symbol.Name,
                                          classDeclarationSyntax.GetAccessType(),
                                          classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));

        return containingContext;
    }
}