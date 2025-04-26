using System;
using Credfeto.Database.Source.Generation.Models;
using Credfeto.Database.Source.Generation.Receivers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Credfeto.Database.Source.Generation;

[Generator(LanguageNames.CSharp)]
public sealed class DatabaseCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(ExtractMethods(context), action: GenerateMethods);
    }

    private static IncrementalValuesProvider<(
        MethodGeneration? methodGeneration,
        InvalidModelInfo? invalidModel,
        ErrorInfo? errorInfo
    )> ExtractMethods(in IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (n, _) => n is MethodDeclarationSyntax,
            transform: DatabaseSyntaxReceiver.GetMethodDetails
        );
    }

    private static void GenerateMethods(
        SourceProductionContext sourceProductionContext,
        (MethodGeneration? methodGeneration, InvalidModelInfo? invalidModel, ErrorInfo? errorInfo) generation
    )
    {
        if (generation.invalidModel is not null)
        {
            ReportInvalidModelError(context: sourceProductionContext, invalidModel: generation.invalidModel.Value);

            return;
        }

        if (generation.errorInfo is not null)
        {
            ErrorInfo errorInfo = generation.errorInfo.Value;
            ReportException(
                location: errorInfo.Location,
                context: sourceProductionContext,
                exception: errorInfo.Exception
            );

            return;
        }

        if (generation.methodGeneration is not null)
        {
            DatabaseSourceCodeGenerator.GenerateOneMethodGroup(
                context: sourceProductionContext,
                generation.methodGeneration
            );
        }
    }

    private static void ReportException(Location location, in SourceProductionContext context, Exception exception)
    {
        context.ReportDiagnostic(
            diagnostic: Diagnostic.Create(
                new(
                    id: "CDSG002",
                    title: "Unhandled Exception",
                    exception.Message + ' ' + exception.StackTrace,
                    category: VersionInformation.Product,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: location
            )
        );
    }

    private static void ReportInvalidModelError(in SourceProductionContext context, in InvalidModelInfo invalidModel)
    {
        context.ReportDiagnostic(
            diagnostic: Diagnostic.Create(
                new(
                    id: "CDSG001",
                    title: "Invalid model",
                    messageFormat: invalidModel.Message,
                    category: VersionInformation.Product,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: invalidModel.Location
            )
        );
    }
}
