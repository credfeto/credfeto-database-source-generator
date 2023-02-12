using System.Collections.Generic;
using System.Linq;
using Credfeto.Database.Interfaces;
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
        MethodInfo methodInfo = GetMethod(context: context, methodDeclarationSyntax: methodDeclarationSyntax);

        this._methods.Add(item: new(containingContext: containingContext, methodInfo: methodInfo));
    }

    private static MethodInfo GetMethod(GeneratorSyntaxContext context, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        IReadOnlyList<string> attributes = methodDeclarationSyntax.AttributeLists.SelectMany(selector: x => x.Attributes)
                                                                  .Where(x => IsSqlObjectMapAttribute(context: context, attributeSyntax: x))
                                                                  .Select(selector: x => x.Name.ToString())
                                                                  .ToList();

        string name = methodDeclarationSyntax.Identifier.Text;

        TypeSyntax returnType = methodDeclarationSyntax.ReturnType;

        string returnTypeName = returnType.ToString();

        if (returnType is GenericNameSyntax genericNameSyntax)
        {
            returnTypeName = genericNameSyntax.TypeArgumentList.Arguments.ToString();
        }

        return new(methodDeclarationSyntax.GetAccessType(),
                   methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword),
                   name: name,
                   returnType: returnTypeName,
                   attributes: attributes,
                   method: methodDeclarationSyntax);
    }

    private static bool IsSqlObjectMapAttribute(in GeneratorSyntaxContext context, AttributeSyntax attributeSyntax)
    {
        if (context.SemanticModel.GetDeclaredSymbol(declaration: attributeSyntax) is not INamedTypeSymbol symbol)
        {
            return true;
        }

        return symbol.ToDisplayString() == typeof(SqlObjectMapAttribute).FullName;
    }

    private static ClassInfo GetClass(in GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(declaration: classDeclarationSyntax)!;

        return new(symbol.ContainingNamespace.ToDisplayString(), name: symbol.Name, classDeclarationSyntax.GetAccessType(), classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
    }
}