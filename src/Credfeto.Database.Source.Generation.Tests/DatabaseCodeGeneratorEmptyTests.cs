using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Tests.Verifiers;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.Source.Generation.Tests;

[SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
public sealed class DatabaseCodeGeneratorEmptyTests : GeneratorVerifierTestsBase<DatabaseCodeGenerator>
{
    public DatabaseCodeGeneratorEmptyTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public Task NothingToGenerateGeneratesNothingAsync()
    {
        const string test = @"
#nullable enable

namespace ConsoleApplication1
{
    public static class EmptyClass
    {
    }
}";

        return this.VerifyAsync(code: test, Array.Empty<(string filename, string generated)>(), cancellationToken: CancellationToken.None);
    }
}