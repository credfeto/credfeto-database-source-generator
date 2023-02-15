using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit.Abstractions;

namespace Credfeto.Database.Source.Generation.Tests.Verifiers;

public abstract class GeneratorVerifierTestsBase<TSourceGenerator> : LoggingTestBase
    where TSourceGenerator : ISourceGenerator, new()
{
    protected GeneratorVerifierTestsBase(ITestOutputHelper output)
        : base(output)
    {
    }

    protected Task VerifyAsync(string code, IReadOnlyList<(string filename, string generated)> expected, in CancellationToken cancellationToken)
    {
        CSharpSourceGeneratorVerifier<TSourceGenerator>.Test t = BuildSources(code: code, expected: expected);

        this.Output.WriteLine(code);

        return t.RunAsync(cancellationToken);
    }

    private static CSharpSourceGeneratorVerifier<TSourceGenerator>.Test BuildSources(string code, IReadOnlyList<(string filename, string generated)> expected)
    {
        CSharpSourceGeneratorVerifier<TSourceGenerator>.Test t = new() { TestState = { Sources = { code } } };

        foreach ((string filename, string generated) in expected)
        {
            (Type sourceGeneratorType, string filename, SourceText content) item = (sourceGeneratorType: typeof(TSourceGenerator), filename,
                content: SourceText.From(generated.ReplaceLineEndings(), encoding: Encoding.UTF8, checksumAlgorithm: SourceHashAlgorithm.Sha256));

            t.TestState.GeneratedSources.Add(item);
        }

        return t;
    }
}