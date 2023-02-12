using System;
using System.Linq;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class AttributeMappings
{
    public static SqlObject? GetSqlObject(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        return methodDeclarationSyntax.AttributeLists.SelectMany(selector: x => x.Attributes)
                                      .Where(x => IsSqlObjectMapAttribute(semanticModel: semanticModel, attributeSyntax: x))
                                      .Select(selector: CreateSqlObject)
                                      .RemoveNulls()
                                      .FirstOrDefault();
    }

    public static MapperInfo? GetMapperInfo(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        return methodDeclarationSyntax.AttributeLists.SelectMany(selector: x => x.Attributes)
                                      .Where(x => IsMapperAttribute(semanticModel: semanticModel, attributeSyntax: x))
                                      .Select(x => CreateMapperInfo(semanticModel: semanticModel, attributeSyntax: x))
                                      .RemoveNulls()
                                      .FirstOrDefault();
    }

    private static MapperInfo? CreateMapperInfo(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        if (attributeSyntax.ArgumentList?.Arguments.Count != 0)
        {
            Console.WriteLine("No arguments");

            return null;
        }

        if (semanticModel.GetSymbol(attributeSyntax) is not INamedTypeSymbol _)
        {
            Console.WriteLine("Is not INamedTypeSymbol");

            return null;
        }

        // if (symbol is GenericNameSyntax genericNameSyntax)
        // {
        //     ISymbol? mapperSymbol = semanticModel.GetSymbol(genericNameSyntax.TypeArgumentList.Arguments[0]);
        //
        //     if (mapperSymbol is null)
        //     {
        //         // todo: throw exception?
        //         return null;
        //     }
        //
        //     ISymbol? mappedSymbol = semanticModel.GetSymbol(genericNameSyntax.TypeArgumentList.Arguments[1]);
        //
        //     if (mappedSymbol is null)
        //     {
        //         // todo: throw exception?
        //         return null;
        //     }
        //
        //     return new MapperInfo(mapperSymbol, mappedSymbol);
        // }

        Console.WriteLine("Banana");

        return null;
    }

    private static bool IsMapperAttribute(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        if (semanticModel.GetSymbol(attributeSyntax) is not INamedTypeSymbol symbol)
        {
            return false;
        }

        if (symbol.ContainingNamespace.ToDisplayString() != "Credfeto.Database.Interfaces")
        {
            return false;
        }

        return symbol.Name is "SqlFieldMap" or "SqlFieldMapAttribute";
    }

    private static bool IsSqlObjectMapAttribute(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        if (semanticModel.GetSymbol(attributeSyntax) is not INamedTypeSymbol symbol)
        {
            return false;
        }

        if (symbol.ContainingNamespace.ToDisplayString() != "Credfeto.Database.Interfaces")
        {
            return false;
        }

        return symbol.Name is "SqlObjectMap" or "SqlObjectMapAttribute";
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
}