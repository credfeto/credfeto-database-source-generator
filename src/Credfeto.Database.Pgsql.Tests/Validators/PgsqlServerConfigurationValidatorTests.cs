using System;
using Credfeto.Database.Pgsql.Validators;
using FunFair.Test.Common;
using Xunit;
using static FunFair.Test.Common.DispatcherCaseData;

namespace Credfeto.Database.Pgsql.Tests.Validators;

public sealed class PgsqlServerConfigurationValidatorTests
    : ValidatorTestBase<PgsqlServerConfigurationValidator, PgsqlServerConfiguration>
{
    public PgsqlServerConfigurationValidatorTests(ITestOutputHelper output)
        : base(output) { }

    public static TheoryData<string, Action<PgsqlServerConfigurationValidatorTests>> BaseCaseData() =>
        BuildDispatcherCases<PgsqlServerConfigurationValidatorTests>().ToTheoryData();

    [Theory]
    [MemberData(nameof(BaseCaseData))]
    public void CommonTests(string name, Action<PgsqlServerConfigurationValidatorTests> action)
    {
        Assert.NotEmpty(name);
        action(this);
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
