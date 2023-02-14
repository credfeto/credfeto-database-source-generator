using System;
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
            MethodGeneration? method = BuildMethod(context: context, methodDeclarationSyntax: methodDeclarationSyntax, classDeclarationSyntax: classDeclarationSyntax);

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

    private static MethodGeneration? BuildMethod(in GeneratorSyntaxContext context, MethodDeclarationSyntax methodDeclarationSyntax, ClassDeclarationSyntax classDeclarationSyntax)
    {
        SqlObject? sqlObject = AttributeMappings.GetSqlObject(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        if (sqlObject is null)
        {
            return null;
        }

        ClassInfo containingContext = GetClass(semanticModel: context.SemanticModel, classDeclarationSyntax: classDeclarationSyntax);
        MethodInfo methodInfo = GetMethod(semanticModel: context.SemanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        return new(containingContext: containingContext, methodInfo: methodInfo, semanticModel: context.SemanticModel, sqlObject: sqlObject);
    }

    private static MethodInfo GetMethod(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclarationSyntax)
    {
        string name = methodDeclarationSyntax.Identifier.Text;

        TypeSyntax returnType = methodDeclarationSyntax.ReturnType;

        MapperInfo? mapperInfo = AttributeMappings.GetMapperInfo(semanticModel: semanticModel, methodDeclarationSyntax: methodDeclarationSyntax);

        MethodReturnType methodReturnType = GetReturnType(semanticModel: semanticModel, returnType: returnType, mapperInfo: mapperInfo, name: name);

        IReadOnlyList<MethodParameter> parameters = GetParameters(semanticModel: semanticModel, method: methodDeclarationSyntax);

        return new(methodDeclarationSyntax.GetAccessType(), methodDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword), name: name, returnType: methodReturnType, parameters: parameters);
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

        ISymbol returnSymbol = semanticModel.GetSymbol(identifierNameSyntax) ?? throw new InvalidOperationException(message: $"Method {name} could not determine task type");

        return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: null, mapperInfo: mapperInfo);
    }

    private static MethodReturnType GetGenericTaskReturnType(SemanticModel semanticModel, MapperInfo? mapperInfo, string name, GenericNameSyntax genericNameSyntax)
    {
        if (genericNameSyntax.Identifier.Text != "Task")
        {
            throw new InvalidOperationException(message: $"Method {name} does not return a Task");
        }

        ISymbol returnSymbol = semanticModel.GetSymbol(genericNameSyntax) ?? throw new InvalidOperationException(message: $"Method {name} could not determine task type");

        TypeSyntax taskReturnType = genericNameSyntax.TypeArgumentList.Arguments[0];

        if (taskReturnType is GenericNameSyntax taskGenericNameSyntax)
        {
            ISymbol taskReturnSymbol = semanticModel.GetSymbol(taskGenericNameSyntax) ?? throw new InvalidOperationException(message: $"Method {name} could not determine task return type");

            TypeSyntax taskReturnElementType = taskGenericNameSyntax.TypeArgumentList.Arguments[0];
            ISymbol taskReturnElementSymbol = semanticModel.GetSymbol(taskReturnElementType) ??
                                              throw new InvalidOperationException(message: $"Method {name} could not determine task return element type");

            return new(returnType: returnSymbol, collectionReturnType: taskReturnSymbol, elementReturnType: taskReturnElementSymbol, mapperInfo: mapperInfo);
        }

        if (taskReturnType is PredefinedTypeSyntax predefinedTypeSyntax)
        {
            ISymbol taskIdentifierPredefinedTypeReturnSymbol =
                semanticModel.GetSymbol(predefinedTypeSyntax) ?? throw new InvalidOperationException(message: $"Method {name} could not determine task return element type");

            return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierPredefinedTypeReturnSymbol, mapperInfo: mapperInfo);
        }

        if (taskReturnType is NullableTypeSyntax nullableTypeSyntax)
        {
            ISymbol taskIdentifierNullableTypeReturnSymbol =
                semanticModel.GetSymbol(nullableTypeSyntax) ?? throw new InvalidOperationException(message: $"Method {name} could not determine task return element type");

            return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierNullableTypeReturnSymbol, mapperInfo: mapperInfo);
        }

        if (taskReturnType is not IdentifierNameSyntax taskIdentifierNameSyntax)
        {
            throw new InvalidOperationException(message: $"Method {name} does not return a Task");
        }

        ISymbol taskIdentifierReturnSymbol = semanticModel.GetSymbol(taskIdentifierNameSyntax) ??
                                             throw new InvalidOperationException(message: $"Method {name} could not determine task return element type");

        return new(returnType: returnSymbol, collectionReturnType: null, elementReturnType: taskIdentifierReturnSymbol, mapperInfo: mapperInfo);
    }

    private static ClassInfo GetClass(SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax)
    {
        INamedTypeSymbol symbol = (INamedTypeSymbol)semanticModel.GetSymbol(classDeclarationSyntax)!;

        return new(symbol.ContainingNamespace.ToDisplayString(), name: symbol.Name, classDeclarationSyntax.GetAccessType(), classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword));
    }

    private static IReadOnlyList<MethodParameter> GetParameters(SemanticModel semanticModel, MethodDeclarationSyntax method)
    {
        static IEnumerable<MethodParameter> Build(SemanticModel model, SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            foreach (ParameterSyntax parameter in parameters)
            {
                yield return GetParameter(semanticModel: model, parameter: parameter);
            }
        }

        return Build(model: semanticModel, parameters: method.ParameterList.Parameters)
            .ToArray();
    }

    private static MethodParameter GetParameter(SemanticModel semanticModel, ParameterSyntax parameter)
    {
        string parameterName = parameter.Identifier.Text;
        ISymbol pType = semanticModel.GetSymbol(parameter) ?? throw new InvalidOperationException(message: $"Could not determine type of parameter {parameterName}");

        MapperInfo? mapperInfo = AttributeMappings.GetMapperInfo(semanticModel: semanticModel, parameterSyntax: parameter);

        string displayType = pType.ToDisplayString();

        return IsContextParameter(displayType)
            ? new(name: parameterName, type: pType, usage: MethodParameterUsage.CONTEXT, mapperInfo: null)
            : new(name: parameterName, type: pType, usage: MethodParameterUsage.DB, mapperInfo: mapperInfo);
    }

    private static bool IsContextParameter(string displayType)
    {
        return displayType == typeof(DbConnection).FullName || displayType == typeof(CancellationToken).FullName;
    }
}