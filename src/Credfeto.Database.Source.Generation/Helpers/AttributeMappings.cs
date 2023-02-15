using System;
using System.Linq;
using System.Threading;
using Credfeto.Database.Source.Generation.Exceptions;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Helpers;

internal static class AttributeMappings
{
    public static SqlObject? GetSqlObject(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken)
    {
        return methodDeclarationSyntax.AttributeLists.SelectMany(selector: x => x.Attributes)
                                      .Select(x => CreateSqlObject(semanticModel: semanticModel, attributeSyntax: x, cancellationToken: cancellationToken))
                                      .RemoveNulls()
                                      .FirstOrDefault();
    }

    public static MapperInfo? GetMapperInfo(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken)
    {
        return GetMapperInfo(semanticModel: semanticModel, attributeLists: methodDeclarationSyntax.AttributeLists, cancellationToken: cancellationToken);
    }

    public static MapperInfo? GetMapperInfo(SemanticModel semanticModel, ParameterSyntax parameterSyntax, CancellationToken cancellationToken)
    {
        return GetMapperInfo(semanticModel: semanticModel, attributeLists: parameterSyntax.AttributeLists, cancellationToken: cancellationToken);
    }

    private static MapperInfo? GetMapperInfo(SemanticModel semanticModel, in SyntaxList<AttributeListSyntax> attributeLists, CancellationToken cancellationToken)
    {
        return attributeLists.SelectMany(selector: x => x.Attributes)
                             .Select(x => CreateMapperInfo(semanticModel: semanticModel, attributeSyntax: x, cancellationToken: cancellationToken))
                             .RemoveNulls()
                             .FirstOrDefault();
    }

    private static MapperInfo? CreateMapperInfo(SemanticModel semanticModel, AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
    {
        ISymbol? symbol = semanticModel.GetSymbol(node: attributeSyntax, cancellationToken: cancellationToken);

        if (symbol == null)
        {
            return null;
        }

        if (symbol.Kind == SymbolKind.ErrorType)
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

        if (mapperSymbol.Kind == SymbolKind.ErrorType)
        {
            throw new InvalidModelException($"Unable to determine the mapped type for {mapperSymbol.ToDisplayString()}");
        }

        ISymbol mappedSymbol = containingType.TypeArguments[1];

        if (mappedSymbol.Kind == SymbolKind.ErrorType)
        {
            throw new InvalidModelException($"Unable to determine the mapped type for {mappedSymbol.ToDisplayString()}");
        }

        return new(mapperSymbol: mapperSymbol, mappedSymbol: mappedSymbol);
    }

    private static SqlObject? CreateSqlObject(SemanticModel semanticModel, AttributeSyntax attributeSyntax, CancellationToken cancellationToken)
    {
        ISymbol? symbol = semanticModel.GetSymbol(node: attributeSyntax, cancellationToken: cancellationToken);

        if (symbol == null)
        {
            return null;
        }

        if (symbol.Kind == SymbolKind.ErrorType)
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

        return new(RemoveQuotes(objectName), (SqlObjectType)Enum.Parse(typeof(SqlObjectType), parts[1], ignoreCase: false));
    }

    private static string RemoveQuotes(string objectName)
    {
        if (objectName.StartsWith(value: "\"", comparisonType: StringComparison.Ordinal) && objectName.EndsWith(value: "\"", comparisonType: StringComparison.Ordinal))
        {
            return objectName.Substring(startIndex: 1, objectName.Length - 2);
        }

        return objectName;
    }
}