using System;
using Credfeto.Database.SqlServer.Validators;
using FunFair.Test.Common;
using Xunit;
using static FunFair.Test.Common.DispatcherCaseData;

namespace Credfeto.Database.SqlServer.Tests.Validators;

public sealed class SqlServerConfigurationValidatorTests
    : ValidatorTestBase<SqlServerConfigurationValidator, SqlServerConfiguration>
{
    public SqlServerConfigurationValidatorTests(ITestOutputHelper output)
        : base(output) { }

    public static TheoryData<string, Action<SqlServerConfigurationValidatorTests>> BaseCaseData() =>
        BuildDispatcherCases<SqlServerConfigurationValidatorTests>().ToTheoryData();

    [Theory]
    [MemberData(nameof(BaseCaseData))]
    public void CommonTests(string name, Action<SqlServerConfigurationValidatorTests> action)
    {
        Assert.NotEmpty(name);
        action(this);
    }

    protected override SqlServerConfiguration CreateAValidObject()
    {
        return new("Database=Example;Server=.;Integrated Security=SSPI");
    }

    protected override void EverythingValid()
    {
        this.TestEverythingValid();
    }

    [Fact]
    public void ConnectionStringCannotBeEmpty()
    {
        SqlServerConfiguration options = new(string.Empty);

        this.Validate(instance: options, expectedErrorCount: 1, nameof(SqlServerConfiguration.ConnectionString));
    }

    [Fact]
    public void ConnectionStringCannotBeInvalid()
    {
        SqlServerConfiguration options = new("Qwertyuiop");

        this.Validate(instance: options, expectedErrorCount: 1, nameof(SqlServerConfiguration.ConnectionString));
    }
}
