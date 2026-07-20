using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Credfeto.Database.Migrations.Source.Generation.Extensions;
using Credfeto.Database.Migrations.Source.Generation.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Credfeto.Database.Migrations.Source.Generation;

[Generator(LanguageNames.CSharp)]
public sealed class DatabaseMigrationsCodeGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "Credfeto.Database.Migrations.DatabaseMigrationsAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AttributedClassInfo> classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            AttributeFullName,
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) =>
                GetClassInfo((ClassDeclarationSyntax)ctx.TargetNode, (INamedTypeSymbol)ctx.TargetSymbol)
        );

        IncrementalValueProvider<ImmutableArray<MigrationSourceFile>> migrationFiles = context
            .AdditionalTextsProvider.Where(static file =>
                file.Path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)
            )
            .SelectMany(static (file, ct) => ToMigrations(file, ct))
            .Collect();

        IncrementalValuesProvider<MigrationGenerationContext> generations = classes
            .Combine(migrationFiles)
            .Select(static (pair, _) => BuildGeneration(classInfo: pair.Left, migrationFiles: pair.Right));

        context.RegisterSourceOutput(generations, action: Generate);
    }

    private static AttributedClassInfo GetClassInfo(ClassDeclarationSyntax classDeclaration, INamedTypeSymbol symbol)
    {
        bool isPartial = classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        string accessModifier = symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        string ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        return new(
            Namespace: ns,
            ClassName: symbol.Name,
            AccessModifier: accessModifier,
            IsPartial: isPartial,
            Location: classDeclaration.Identifier.GetLocation()
        );
    }

    private static ImmutableArray<MigrationSourceFile> ToMigrations(
        AdditionalText file,
        CancellationToken cancellationToken
    )
    {
        SourceText? sourceText = file.GetText(cancellationToken);

        if (sourceText is null)
        {
            return [];
        }

        string fileName = Path.GetFileName(file.Path);

        return MigrationFileNameExtensions.TryParse(fileName, sourceText, out MigrationSourceFile migration)
            ? [migration]
            : [];
    }

    private static MigrationGenerationContext BuildGeneration(
        in AttributedClassInfo classInfo,
        in ImmutableArray<MigrationSourceFile> migrationFiles
    )
    {
        if (!classInfo.IsPartial)
        {
            return new(
                ClassGeneration: null,
                InvalidModel: new(
                    Message: $"Type '{classInfo.ClassName}' must be declared 'partial' to use [DatabaseMigrations].",
                    Location: classInfo.Location
                ),
                Duplicates: []
            );
        }

        Location location = classInfo.Location;

        ImmutableArray<DuplicateMigrationIdInfo> duplicates =
        [
            .. migrationFiles
                .GroupBy(static m => m.Id)
                .Where(static g => g.Skip(1).Any())
                .Select(g => new DuplicateMigrationIdInfo(Id: g.Key, Location: location)),
        ];

        if (!duplicates.IsEmpty)
        {
            return new(ClassGeneration: null, InvalidModel: null, Duplicates: duplicates);
        }

        ImmutableArray<MigrationSourceFile> ordered = [.. migrationFiles.OrderBy(static m => m.Id)];

        return new(
            ClassGeneration: new(
                Namespace: classInfo.Namespace,
                ClassName: classInfo.ClassName,
                AccessModifier: classInfo.AccessModifier,
                Migrations: ordered
            ),
            InvalidModel: null,
            Duplicates: []
        );
    }

    private static void Generate(SourceProductionContext context, MigrationGenerationContext generation)
    {
        if (generation.InvalidModel is not null)
        {
            ReportInvalidModel(context: context, invalidModel: generation.InvalidModel.Value);

            return;
        }

        foreach (DuplicateMigrationIdInfo duplicate in generation.Duplicates)
        {
            ReportDuplicateMigrationId(context: context, duplicate: duplicate);
        }

        if (generation.ClassGeneration is not null)
        {
            MigrationSourceCodeGenerator.Generate(context, generation.ClassGeneration.Value);
        }
    }

    private static void ReportInvalidModel(in SourceProductionContext context, in InvalidModelInfo invalidModel)
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

    private static void ReportDuplicateMigrationId(
        in SourceProductionContext context,
        in DuplicateMigrationIdInfo duplicate
    )
    {
        context.ReportDiagnostic(
            diagnostic: Diagnostic.Create(
                new(
                    id: RuleConstants.DuplicateMigrationId,
                    title: "Duplicate migration id",
                    messageFormat: $"Multiple migration files declare id {duplicate.Id.ToString(CultureInfo.InvariantCulture)}.",
                    category: VersionInformation.Product,
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                ),
                location: duplicate.Location
            )
        );
    }
}
