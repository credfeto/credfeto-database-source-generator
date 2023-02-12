using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Credfeto.Database.Source.Generation.Tests.Verifiers;
using Xunit;

namespace Credfeto.Database.Source.Generation.Tests;

[SuppressMessage(category: "Meziantou.Analyzer", checkId: "MA0051:Method too long", Justification = "Test")]
public sealed class DatabaseCodeGeneratorEmptyTests : GeneratorVerifierTestsBase<DatabaseCodeGenerator>
{
    [Fact]
    public Task NothingToGenerateGeneratesNothingAsync()
    {
        const string test = @"
    namespace ConsoleApplication1
    {
        public static class EmptyClass
        {
        }
    }";

        return VerifyAsync(code: test, Array.Empty<(string filename, string generated)>());
    }
}