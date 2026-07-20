using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Credfeto.Database.Migrations.Source.Generation.Tests;

internal static class CompilationHelpers
{
    private static readonly IReadOnlyList<MetadataReference> SharedReferences = BuildReferences();

    public static CSharpCompilation CreateCompilation(string source)
    {
        // Ensure the source has a namespace to avoid '<global namespace>' in hint names
        string wrappedSource = source.Contains("namespace ", StringComparison.Ordinal)
            ? source
            : "namespace TestMigrations;\n" + source;

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(text: wrappedSource, cancellationToken: CancellationToken.None)],
            references: SharedReferences,
            options: new CSharpCompilationOptions(
                outputKind: OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable
            )
        );
    }

    public static GeneratorDriverRunResult RunGenerator(
        string source,
        params (string FileName, string Content)[] additionalFiles
    )
    {
        CSharpCompilation compilation = CreateCompilation(source);

        IReadOnlyList<AdditionalText> additionalTexts =
        [
            .. additionalFiles.Select(f => new InMemoryAdditionalText(path: f.FileName, content: f.Content)),
        ];

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new DatabaseMigrationsCodeGenerator().AsSourceGenerator()],
            additionalTexts: additionalTexts,
            parseOptions: null,
            optionsProvider: null
        );

        return driver
            .RunGeneratorsAndUpdateCompilation(
                compilation: compilation,
                outputCompilation: out _,
                diagnostics: out _,
                cancellationToken: CancellationToken.None
            )
            .GetRunResult();
    }

    private static IReadOnlyList<MetadataReference> BuildReferences()
    {
        // Collect references from all currently-loaded non-dynamic assemblies
        // plus the Migrations assembly explicitly
        HashSet<string> locations = new(StringComparer.OrdinalIgnoreCase);
        List<MetadataReference> references = [];

        // Add the migrations assembly first (guaranteed)
        AddReference(typeof(DatabaseMigrationsAttribute).Assembly.Location);

        // Add any other loaded assemblies
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            {
                AddReference(assembly.Location);
            }
        }

        return references;

        void AddReference(string location)
        {
            if (!string.IsNullOrEmpty(location) && locations.Add(location))
            {
                references.Add(MetadataReference.CreateFromFile(location));
            }
        }
    }
}
