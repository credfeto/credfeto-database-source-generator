using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        SqlObject? sqlObject = GetSqlObject(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        if (sqlObject is null)
        {
            return;
        }

        ClassInfo containingContext = GetClass(semanticModel: context.SemanticModel, classDeclarationSyntax: classDeclarationSyntax);
        MethodInfo methodInfo = GetMethod(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        this._methods.Add(item: new(containingContext: containingContext, methodInfo: methodInfo, semanticModel: context.SemanticModel, sqlObject: sqlObject));
    }

    private static SqlObject? GetSqlObject(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        return methodDeclarationSyntax.AttributeLists.SelectMany(selector: x => x.Attributes)
                                      .Where(x => IsSqlObjectMapAttribute(semanticModel: semanticModel, attributeSyntax: x))
                                      .Select(selector: CreateSqlObject)
                                      .RemoveNulls()
                                      .FirstOrDefault();
    }

    private static SqlObject? CreateSqlObject(AttributeSyntax attributeSyntax)
    {
        if (attributeSyntax.ArgumentList?.Arguments.Count != 2)
        {
            return null;
        }

        string objectName = attributeSyntax.ArgumentList.Arguments[0]
                                           .Expression.ToString();
        string type = attributeSyntax.ArgumentList.Arguments[1]
                                     .Expression.ToString();

        string[] parts = type.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        return new(name: objectName, (SqlObjectType)Enum.Parse(typeof(SqlObjectType), parts[1], ignoreCase: false));
    }

    private static MethodInfo GetMethod(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        string name = methodDeclarationSyntax.Identifier.Text;

        TypeSyntax returnType = methodDeclarationSyntax.ReturnType;

        StringBuilder sb = new();
        sb.Append("Base: ")
          .AppendLine(returnType.ToString());

        if (returnType is GenericNameSyntax genericNameSyntax)
        {
            if (genericNameSyntax.Identifier.Text != "Task")
            {
                throw new InvalidOperationException(message: $"Method {name} does not return a Task");
            }

            ISymbol? returnSymbol = semanticModel.GetSymbol(genericNameSyntax);

            if (returnSymbol != null)
            {
                sb.Append("Task: ")
                  .AppendLine(returnSymbol.ToDisplayString());
            }

            TypeSyntax taskReturnType = genericNameSyntax.TypeArgumentList.Arguments[0];
            sb.Append("Task Return: ")
              .AppendLine(taskReturnType.ToString());

            if (taskReturnType is GenericNameSyntax taskGenericNameSyntax)
            {
                ISymbol? taskReturnSymbol = semanticModel.GetSymbol(taskGenericNameSyntax);

                if (taskReturnSymbol != null)
                {
                    sb.Append("Task Return Symbol: ")
                      .AppendLine(taskReturnSymbol.ToDisplayString());

                    TypeSyntax taskReturnElementType = taskGenericNameSyntax.TypeArgumentList.Arguments[0];
                    ISymbol? taskReturnElementSymbol = semanticModel.GetSymbol(taskReturnElementType);

                    if (taskReturnElementSymbol != null)
                    {
                        sb.Append("Task Return Element Symbol: ")
                          .AppendLine(taskReturnElementSymbol.ToDisplayString());
                    }
                }
            }
        }

        return new(methodDeclarationSyntax.GetAccessType(), methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword), name: name, sb.ToString(), method: methodDeclarationSyntax);
    }

    private static bool IsSqlObjectMapAttribute(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        if (semanticModel.GetSymbol(attributeSyntax) is not INamedTypeSymbol symbol)
        {
            return true;
        }

        Console.WriteLine(symbol.ContainingNamespace.ToDisplayString());

        if (symbol.ContainingNamespace.ToDisplayString() != "Credfeto.Database.Interfaces")
        {
            return false;
        }

        return symbol.Name is "SqlObjectMap" or "SqlObjectMapAttribute";
    }

    private static ClassInfo GetClass(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)semanticModel.GetSymbol(classDeclarationSyntax)!;

        return new(symbol.ContainingNamespace.ToDisplayString(), name: symbol.Name, classDeclarationSyntax.GetAccessType(), classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
    }
}