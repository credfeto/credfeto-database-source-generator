using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Credfeto.Database.Source.Generation.Tests;

internal static class CompilationHelpers
{
    private static readonly IReadOnlyList<MetadataReference> SharedReferences = BuildReferences();

    public static CSharpCompilation CreateCompilation(string source)
    {
        // Ensure the source has a namespace to avoid '<global namespace>' in hint names
        string wrappedSource = source.Contains("namespace ", StringComparison.Ordinal)
            ? source
            : "namespace TestDatabase;\n" + source;

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

    public static GeneratorDriverRunResult RunGenerator(string source)
    {
        CSharpCompilation compilation = CreateCompilation(source);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DatabaseCodeGenerator());

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
        // plus the Interfaces assembly explicitly
        HashSet<string> locations = new(StringComparer.OrdinalIgnoreCase);
        List<MetadataReference> references = [];

        // Add the interfaces assembly first (guaranteed)
        AddReference(typeof(SqlObjectMapAttribute).Assembly.Location);

        // Add BCL assemblies by anchoring on well-known types
        AddReference(typeof(object).Assembly.Location);
        AddReference(typeof(Task).Assembly.Location);
        AddReference(typeof(DbConnection).Assembly.Location);
        AddReference(typeof(RuntimeHelpers).Assembly.Location);
        AddReference(typeof(Console).Assembly.Location);
        AddReference(typeof(Enumerable).Assembly.Location);
        AddReference(typeof(System.Collections.Generic.List<>).Assembly.Location);

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
