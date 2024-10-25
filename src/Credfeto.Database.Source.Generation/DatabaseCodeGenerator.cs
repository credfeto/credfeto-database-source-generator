using System;
using System.Threading;
using Credfeto.Database.Source.Generation.Exceptions;
using Credfeto.Database.Source.Generation.Models;
using Credfeto.Database.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation;

[Generator(LanguageNames.CSharp)]
public sealed class DatabaseCodeGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(ExtractMethods(context), action: GenerateMethods);
    }

    private static IncrementalValuesProvider<(MethodGeneration? methodGeneration, InvalidModelInfo? invalidModel, ErrorInfo? errorInfo)> ExtractMethods(in IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(predicate: static (n, _) => n is MethodDeclarationSyntax, transform: GetMethodDetails);
    }

    private static readonly (MethodGeneration? methodGeneration, InvalidModelInfo? invalidModel, ErrorInfo? errorInfo) IgnoredMethod = (methodGeneration: null, invalidModel: null, errorInfo: null);

    private static (MethodGeneration? methodGeneration, InvalidModelInfo? invalidModel, ErrorInfo? errorInfo) GetMethodDetails(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        if (generatorSyntaxContext.Node is not MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return IgnoredMethod;
        }

        if (methodDeclarationSyntax.AttributeLists.Count == 0)
        {
            return IgnoredMethod;
        }

        if (!methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return IgnoredMethod;
        }

        ClassDeclarationSyntax? classDeclarationSyntax = DatabaseSyntaxReceiver.GetClassDeclarationSyntax(methodDeclarationSyntax);

        if (classDeclarationSyntax is null)
        {
            return IgnoredMethod;
        }

        if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return IgnoredMethod;
        }

        Location location = generatorSyntaxContext.Node.GetLocation();

        try
        {
            MethodGeneration? method = DatabaseSyntaxReceiver.BuildMethod(context: generatorSyntaxContext,
                                                   methodDeclarationSyntax: methodDeclarationSyntax,
                                                   classDeclarationSyntax: classDeclarationSyntax,
                                                   cancellationToken: cancellationToken);

            return method is null
                ? IgnoredMethod
                : (methodGeneration:method, invalidModel: null, errorInfo:null);
        }
        catch (InvalidModelException exception)
        {
            return (methodGeneration:null, invalidModel: new InvalidModelInfo(location: location, exception.Message), errorInfo: null);
        }
        catch (Exception exception)
        {
            return ( methodGeneration:null, invalidModel: null, errorInfo:new ErrorInfo(location: location, exception: exception));
        }
    }

    private static void GenerateMethods(SourceProductionContext sourceProductionContext, (MethodGeneration? methodGeneration, InvalidModelInfo? invalidModel, ErrorInfo? errorInfo) generation)
    {
        if(generation.invalidModel is not null)
        {
            ReportInvalidModelError(context: sourceProductionContext, generation.invalidModel.Value);

            return;
        }
        if (generation.errorInfo is not null)
        {
            ErrorInfo errorInfo = generation.errorInfo.Value;
            ReportException(location: errorInfo.Location, context: sourceProductionContext, exception: errorInfo.Exception);

            return;
        }

        if (generation.methodGeneration is not null)
        {
            GenerateMethod(context: sourceProductionContext, methodGeneration: generation.methodGeneration);
        }
    }

    private static void GenerateMethod(in SourceProductionContext context, in MethodGeneration methodGeneration)
    {
        DatabaseSourceCodeGenerator.GenerateOneMethodGroup(context: context,  [methodGeneration], methodGeneration.MethodGrouping);
    }

    private static void ReportException(Location location, in SourceProductionContext context, Exception exception)
    {
        context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "CDSG002",
                                                                   title: "Unhandled Exception",
                                                                   exception.Message + ' ' + exception.StackTrace,
                                                                   category: VersionInformation.Product,
                                                                   defaultSeverity: DiagnosticSeverity.Error,
                                                                   isEnabledByDefault: true),
                                                               location: location));
    }

    private static void ReportInvalidModelError(in SourceProductionContext context, in InvalidModelInfo invalidModel)
    {
        context.ReportDiagnostic(diagnostic: Diagnostic.Create(new(id: "CDSG001",
                                                                   title: "Invalid model",
                                                                   messageFormat: invalidModel.Message,
                                                                   category: VersionInformation.Product,
                                                                   defaultSeverity: DiagnosticSeverity.Error,
                                                                   isEnabledByDefault: true),
                                                               location: invalidModel.Location));

    }
}