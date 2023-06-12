using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using Credfeto.Database.Source.Generation.Exceptions;
using Credfeto.Database.Source.Generation.Extensions;
using Credfeto.Database.Source.Generation.Helpers;
using Credfeto.Database.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation.Receivers;

internal sealed class DatabaseSyntaxReceiver : ISyntaxContextReceiver
{
    private readonly List<InvalidModelInfo> _errors;
    private readonly List<MethodGeneration> _methods;

    public DatabaseSyntaxReceiver()
    {
        this._methods = new();
        this._errors = new();
    }

    public IReadOnlyList<MethodGeneration> Methods => this._methods;

    public IReadOnlyList<InvalidModelInfo> Errors => this._errors;

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        CancellationToken cancellationToken = CancellationToken.None;

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

        Location location = context.Node.GetLocation();

        try
        {
            MethodGeneration? method = BuildMethod(context: context,
                                                   methodDeclarationSyntax: methodDeclarationSyntax,
                                                   classDeclarationSyntax: classDeclarationSyntax,
                                                   cancellationToken: cancellationToken);

            if (method is null)
            {
                return;
            }

            this._methods.Add(item: method);
        }
        catch (InvalidModelException exception)
        {
            this._errors.Add(new(location: location, message: exception.Message));
        }
    }

    private static MethodGeneration? BuildMethod(in GeneratorSyntaxContext context,
                                                 MethodDeclarationSyntax methodDeclarationSyntax,
                                                 ClassDeclarationSyntax classDeclarationSyntax,
                                                 CancellationToken cancellationToken)
    {
        SqlObject? sqlObject = AttributeMappings.GetSqlObject(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax, cancellationToken: cancellationToken);

        if (sqlObject is null)
        {
            return null;
        }

        ClassInfo containingContext = GetClass(semanticModel: context.SemanticModel, classDeclarationSyntax: classDeclarationSyntax, cancellationToken: cancellationToken);
        MethodToGenerate methodToGenerate = GetMethod(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax, cancellationToken: cancellationToken);

        return new(containingContext: containingContext, methodToGenerate: methodToGenerate, semanticModel: context.SemanticModel, sqlObject: sqlObject, methodDeclarationSyntax.GetLocation());
    }

    private static MethodToGenerate GetMethod(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax, CancellationToken cancellationToken)
    {
        string name = methodDeclarationSyntax.Identifier.Text;

        TypeSyntax returnType = methodDeclarationSyntax.ReturnType;

        MapperInfo? mapperInfo = AttributeMappings.GetMapperInfo(semanticModel: semanticModel, methodDeclarationSyntax: methodDeclarationSyntax, cancellationToken: cancellationToken);

        MethodReturnType methodReturnType = GetReturnType(semanticModel: semanticModel, returnType: returnType, mapperInfo: mapperInfo, name: name, cancellationToken: cancellationToken);

        IReadOnlyList<MethodParameter> parameters = GetParameters(semanticModel: semanticModel, method: methodDeclarationSyntax, cancellationToken: cancellationToken);

        return new(methodDeclarationSyntax.GetAccessType(), methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword), name: name, returnType: methodReturnType, parameters: parameters);
    }

    private static MethodReturnType GetReturnType(SemanticModel semanticModel, TypeSyntax returnType, MapperInfo? mapperInfo, string name, CancellationToken cancellationToken)
    {
        if (returnType is GenericNameSyntax genericNameSyntax)
        {
            return GetGenericTaskReturnType(semanticModel: semanticModel, mapperInfo: mapperInfo, name: name, genericNameSyntax: genericNameSyntax, cancellationToken: cancellationToken);
        }

        if (returnType is IdentifierNameSyntax identifierNameSyntax)
        {
            return GetNonGenericMethodReturnType(semanticModel: semanticModel, mapperInfo: mapperInfo, name: name, identifierNameSyntax: identifierNameSyntax, cancellationToken: cancellationToken);
        }

        throw new InvalidModelException(message: $"Method {name} does not return a Task");
    }

    private static MethodReturnType GetNonGenericMethodReturnType(SemanticModel semanticModel,
                                                                  MapperInfo? mapperInfo,
                                                                  string name,
                                                                  IdentifierNameSyntax identifierNameSyntax,
                                                                  CancellationToken cancellationToken)
    {
        if (identifierNameSyntax.Identifier.Text != "Task" && identifierNameSyntax.Identifier.Text != "ValueTask")
        {
            throw new InvalidModelException(message: $"Method {name} does not return a Task or ValueTask");
        }

        ISymbol returnSymbol = ValidateSymbol(semanticModel.GetSymbol(node: identifierNameSyntax, cancellationToken: cancellationToken), $"Method {name} could not determine task type");

        return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: null, mapperInfo: mapperInfo, isNullable: false);
    }

    private static ISymbol ValidateSymbol(ISymbol? symbol, string error)
    {
        if (symbol is null)
        {
            throw new InvalidModelException(message: error);
        }

        if (symbol.Kind == SymbolKind.ErrorType)
        {
            throw new InvalidModelException(message: error);
        }

        return symbol;
    }

    private static MethodReturnType GetGenericTaskReturnType(SemanticModel semanticModel, MapperInfo? mapperInfo, string name, GenericNameSyntax genericNameSyntax, CancellationToken cancellationToken)
    {
        if (genericNameSyntax.Identifier.Text != "Task" && genericNameSyntax.Identifier.Text != "ValueTask")
        {
            throw new InvalidModelException(message: $"Method {name} does not return a Task or ValueTask");
        }

        ISymbol returnSymbol = ValidateSymbol(semanticModel.GetSymbol(node: genericNameSyntax, cancellationToken: cancellationToken), $"Method {name} could not determine task type");

        TypeSyntax taskReturnType = genericNameSyntax.TypeArgumentList.Arguments[0];

        if (taskReturnType is GenericNameSyntax taskGenericNameSyntax)
        {
            ISymbol taskReturnSymbol = ValidateSymbol(semanticModel.GetSymbol(node: taskGenericNameSyntax, cancellationToken: cancellationToken),
                                                      $"Method {name} could not determine task return type");

            TypeSyntax taskReturnElementType = taskGenericNameSyntax.TypeArgumentList.Arguments[0];
            ISymbol taskReturnElementSymbol = ValidateSymbol(semanticModel.GetSymbol(node: taskReturnElementType, cancellationToken: cancellationToken),
                                                             $"Method {name} could not determine task return element type");

            return new(returnType: returnSymbol, collectionReturnType: taskReturnSymbol, elementReturnType: taskReturnElementSymbol, mapperInfo: mapperInfo, isNullable: false);
        }

        if (taskReturnType is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            ISymbol taskIdentifierPredefinedTypeReturnSymbol = ValidateSymbol(semanticModel.GetSymbol(node: predefinedTypeSyntax, cancellationToken: cancellationToken),
                                                                              $"Method {name} could not determine task return element type");

            return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierPredefinedTypeReturnSymbol, mapperInfo: mapperInfo, isNullable: false);
        }

        if (taskReturnType is NullableTypeSyntax nullableTypeSyntax)
        {
            ISymbol taskIdentifierNullableTypeReturnSymbol = ValidateSymbol(semanticModel.GetSymbol(node: nullableTypeSyntax.ElementType, cancellationToken: cancellationToken),
                                                                            $"Method {name} could not determine task return element type");

            return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierNullableTypeReturnSymbol, mapperInfo: mapperInfo, isNullable: true);
        }

        if (taskReturnType is not IdentifierNameSyntax taskIdentifierNameSyntax)
        {
            throw new InvalidModelException(message: $"Method {name} does not return a Task");
        }

        ISymbol taskIdentifierReturnSymbol = ValidateSymbol(semanticModel.GetSymbol(node: taskIdentifierNameSyntax, cancellationToken: cancellationToken),
                                                            $"Method {name} could not determine task return element type");

        return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierReturnSymbol, mapperInfo: mapperInfo, isNullable: false);
    }

    private static ClassInfo GetClass(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)ValidateSymbol(semanticModel.GetSymbol(node: classDeclarationSyntax, cancellationToken: cancellationToken),
                                                                   $"Could not determine class type for {classDeclarationSyntax.Identifier.Text}");

        return new(symbol.ContainingNamespace.ToDisplayString(), name: symbol.Name, classDeclarationSyntax.GetAccessType(), classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
    }

    private static IReadOnlyList<MethodParameter> GetParameters(SemanticModel semanticModel, MethodDeclarationSyntax method, CancellationToken cancellationToken)
    {
        static IEnumerable<MethodParameter> Build(SemanticModel model, SeparatedSyntaxList<ParameterSyntax> parameters, CancellationToken ct)
        {
            foreach (ParameterSyntax parameter in parameters)
            {
                yield return GetParameter(semanticModel: model, parameter: parameter, cancellationToken: ct);
            }
        }

        return Build(model: semanticModel, parameters: method.ParameterList.Parameters, ct: cancellationToken)
            .ToArray();
    }

    private static MethodParameter GetParameter(SemanticModel semanticModel, ParameterSyntax parameter, CancellationToken cancellationToken)
    {
        string parameterName = parameter.Identifier.Text;
        ISymbol pType = ValidateSymbol(semanticModel.GetSymbol(node: parameter, cancellationToken: cancellationToken), $"Could not determine type of parameter {parameterName}");

        MapperInfo? mapperInfo = AttributeMappings.GetMapperInfo(semanticModel: semanticModel, parameterSyntax: parameter, cancellationToken: cancellationToken);

        string displayType = GetParameterType(pType: pType);

        return IsContextParameter(displayType)
            ? new(name: parameterName, type: pType, usage: MethodParameterUsage.CONTEXT, mapperInfo: null)
            : new(name: parameterName, type: pType, usage: MethodParameterUsage.DB, mapperInfo: mapperInfo);
    }

    private static string GetParameterType(ISymbol pType)
    {
        if (pType is IParameterSymbol ps)
        {
            return ps.Type.ToDisplayString();
        }

        return pType.ToDisplayString();
    }

    private static bool IsContextParameter(string displayType)
    {
        return displayType == typeof(DbConnection).FullName || displayType == typeof(CancellationToken).FullName;
    }
}