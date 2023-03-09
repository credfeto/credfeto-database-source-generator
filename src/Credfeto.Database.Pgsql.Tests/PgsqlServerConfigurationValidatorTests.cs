using FunFair.Test.Common;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.Pgsql.Tests;

public sealed class PgsqlServerConfigurationValidatorTests : ValidatorTestBase<PgsqlServerConfigurationValidator, PgsqlServerConfiguration>
{
    public PgsqlServerConfigurationValidatorTests(ITestOutputHelper output)
        : base(output)
    {
    }

    protected override PgsqlServerConfiguration CreateAValidObject()
    {
        return new("Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=NotTellingYou");
    }

    protected override void EverythingValid()
    {
        this.TestEverythingValid();
    }

    [Fact]
    public void ConnectionStringCannotBeEmpty()
    {
        PgsqlServerConfiguration options = new(string.Empty);

        this.Validate(instance: options, expectedErrorCount: 1, nameof(PgsqlServerConfiguration.ConnectionString));
    }

    [Fact]
    public void ConnectionStringCannotBeInvalid()
    {
        PgsqlServerConfiguration options = new("Qwertyuiop");

        this.Validate(instance: options, expectedErrorCount: 1, nameof(PgsqlServerConfiguration.ConnectionString));
    }
}