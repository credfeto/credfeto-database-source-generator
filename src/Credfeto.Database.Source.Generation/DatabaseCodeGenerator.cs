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

    private static IncrementalValuesProvider<MethodContext> ExtractMethods(in IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (n, _) => n is MethodDeclarationSyntax,
            transform: DatabaseSyntaxReceiver.GetMethodDetails
        );
    }

    private static void GenerateMethods(
        SourceProductionContext sourceProductionContext,
        MethodContext generation
    )
    {
        if (generation.Warnings is not null)
        {
            foreach (WarningModelInfo warning in generation.Warnings)
            {
                ReportWarning(context: sourceProductionContext, warningModelInfo:warning);
            }
        }

        if (generation.InvalidModel is not null)
        {
            ReportInvalidModelError(context: sourceProductionContext, invalidModel: generation.InvalidModel.Value);

            return;
        }

        if (generation.ErrorInfo is not null)
        {
            ErrorInfo errorInfo = generation.ErrorInfo.Value;
            ReportException(
                location: errorInfo.Location,
                context: sourceProductionContext,
                exception: errorInfo.Exception
            );

            return;
        }

        if (generation.MethodGeneration is not null)
        {
            DatabaseSourceCodeGenerator.GenerateOneMethodGroup(
                context: sourceProductionContext,
                generation.MethodGeneration
            );
        }
    }

    private static void ReportException(Location location, in SourceProductionContext context, Exception exception)
    {
        context.ReportDiagnostic(
            diagnostic: Diagnostic.Create(
                new(
                    id: RuleConstants.UnhandledException,
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
                    id: RuleConstants.InvalidModel,
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

    private static void ReportWarning(in SourceProductionContext context, in WarningModelInfo warningModelInfo)
    {
        context.ReportDiagnostic(
            diagnostic: Diagnostic.Create(
                new(
                    id: warningModelInfo.Code,
                    title: warningModelInfo.Message,
                    messageFormat: warningModelInfo.Message,
                    category: VersionInformation.Product,
                    defaultSeverity: DiagnosticSeverity.Info,
                    isEnabledByDefault: true
                ),
                location: warningModelInfo.Location
            )
        );
    }
}
