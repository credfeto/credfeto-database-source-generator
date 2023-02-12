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
                                      .Select(x => CreateSqlObject(semanticModel: semanticModel, attributeSyntax: x))
                                      .RemoveNulls()
                                      .FirstOrDefault();
    }

    public static MapperInfo? GetMapperInfo(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        return methodDeclarationSyntax.AttributeLists.SelectMany(selector: x => x.Attributes)
                                      .Select(x => CreateMapperInfo(semanticModel: semanticModel, attributeSyntax: x))
                                      .RemoveNulls()
                                      .FirstOrDefault();
    }

    private static MapperInfo? CreateMapperInfo(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        ISymbol? symbol = semanticModel.GetSymbol(attributeSyntax);

        if (symbol == null)
        {
            return null;
        }

        INamedTypeSymbol? containingType = symbol.ContainingType;

        if (containingType == null)
        {
            return null;
        }

        if (!containingType.IsGenericType)
        {
            return null;
        }

        string name = containingType.OriginalDefinition.ToDisplayString();

        if (name != "Credfeto.Database.Interfaces.SqlFieldMapAttribute<TM, TD>")
        {
            return null;
        }

        ISymbol mapperSymbol = containingType.TypeArguments[0];

        ISymbol mappedSymbol = containingType.TypeArguments[1];

        return new(mapperSymbol: mapperSymbol, mappedSymbol: mappedSymbol);
    }

    private static SqlObject? CreateSqlObject(SemanticModel semanticModel, AttributeSyntax attributeSyntax)
    {
        ISymbol? symbol = semanticModel.GetSymbol(attributeSyntax);

        if (symbol == null)
        {
            return null;
        }

        string name = symbol.ContainingType.ToDisplayString();

        if (name != "Credfeto.Database.Interfaces.SqlObjectMapAttribute")
        {
            return null;
        }

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