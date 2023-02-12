using System;
using System.Collections.Generic;
using System.Linq;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Helpers;
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

        SqlObject? sqlObject = AttributeMappings.GetSqlObject(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        if (sqlObject is null)
        {
            return;
        }

        ClassInfo containingContext = GetClass(semanticModel: context.SemanticModel, classDeclarationSyntax: classDeclarationSyntax);
        MethodInfo methodInfo = GetMethod(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        this._methods.Add(item: new(containingContext: containingContext, methodInfo: methodInfo, semanticModel: context.SemanticModel, sqlObject: sqlObject));
    }

    private static MethodInfo GetMethod(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        string name = methodDeclarationSyntax.Identifier.Text;

        TypeSyntax returnType = methodDeclarationSyntax.ReturnType;

        MapperInfo? mapperInfo = AttributeMappings.GetMapperInfo(semanticModel: semanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        MethodReturnType methodReturnType = GetReturnType(semanticModel: semanticModel, returnType: returnType, mapperInfo: mapperInfo, name: name);

        return new(methodDeclarationSyntax.GetAccessType(), methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword), name: name, returnType: methodReturnType, method: methodDeclarationSyntax);
    }

    private static MethodReturnType GetReturnType(SemanticModel semanticModel, TypeSyntax returnType, MapperInfo? mapperInfo, string name)
    {
        if (returnType is GenericNameSyntax genericNameSyntax)
        {
            return GetGenericTaskReturnType(semanticModel: semanticModel, mapperInfo: mapperInfo, name: name, genericNameSyntax: genericNameSyntax);
        }

        if (returnType is IdentifierNameSyntax identifierNameSyntax)
        {
            return GetNonGenericMethodReturnType(semanticModel: semanticModel, mapperInfo: mapperInfo, name: name, identifierNameSyntax: identifierNameSyntax);
        }

        throw new InvalidOperationException(message: $"Method {name} does not return a Task");
    }

    private static MethodReturnType GetNonGenericMethodReturnType(SemanticModel semanticModel, MapperInfo? mapperInfo, string name, IdentifierNameSyntax identifierNameSyntax)
    {
        if (identifierNameSyntax.Identifier.Text != "Task")
        {
            throw new InvalidOperationException(message: $"Method {name} does not return a Task");
        }

        ISymbol? returnSymbol = semanticModel.GetSymbol(identifierNameSyntax);

        if (returnSymbol == null)
        {
            throw new InvalidOperationException(message: $"Method {name} could not determine task type");
        }

        return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: null, mapperInfo: mapperInfo);
    }

    private static MethodReturnType GetGenericTaskReturnType(SemanticModel semanticModel, MapperInfo? mapperInfo, string name, GenericNameSyntax genericNameSyntax)
    {
        if (genericNameSyntax.Identifier.Text != "Task")
        {
            throw new InvalidOperationException(message: $"Method {name} does not return a Task");
        }

        ISymbol? returnSymbol = semanticModel.GetSymbol(genericNameSyntax);

        if (returnSymbol == null)
        {
            throw new InvalidOperationException(message: $"Method {name} could not determine task type");
        }

        TypeSyntax taskReturnType = genericNameSyntax.TypeArgumentList.Arguments[0];

        if (taskReturnType is GenericNameSyntax taskGenericNameSyntax)
        {
            ISymbol? taskReturnSymbol = semanticModel.GetSymbol(taskGenericNameSyntax);

            if (taskReturnSymbol == null)
            {
                throw new InvalidOperationException(message: $"Method {name} could not determine task return type");
            }

            TypeSyntax taskReturnElementType = taskGenericNameSyntax.TypeArgumentList.Arguments[0];
            ISymbol? taskReturnElementSymbol = semanticModel.GetSymbol(taskReturnElementType);

            if (taskReturnElementSymbol == null)
            {
                throw new InvalidOperationException(message: $"Method {name} could not determine task return element type");
            }

            return new(returnType: returnSymbol, collectionReturnType: taskReturnSymbol, elementReturnType: taskReturnElementSymbol, mapperInfo: mapperInfo);
        }

        if (taskReturnType is not IdentifierNameSyntax taskIdentifierNameSyntax)
        {
            throw new InvalidOperationException(message: $"Method {name} does not return a Task");
        }

        ISymbol? taskIdentifierReturnSymbol = semanticModel.GetSymbol(taskIdentifierNameSyntax);

        if (taskIdentifierReturnSymbol == null)
        {
            throw new InvalidOperationException(message: $"Method {name} could not determine task return element type");
        }

        return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierReturnSymbol, mapperInfo: mapperInfo);
    }

    private static ClassInfo GetClass(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)semanticModel.GetSymbol(classDeclarationSyntax)!;

        return new(symbol.ContainingNamespace.ToDisplayString(), name: symbol.Name, classDeclarationSyntax.GetAccessType(), classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
    }
}