using Credfeto.Database.SqlServer.Validators;
using FunFair.Test.Common;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.SqlServer.Tests.Validators;

public sealed class SqlServerConfigurationValidatorTests
    : ValidatorTestBase<SqlServerConfigurationValidator, SqlServerConfiguration>
{
    public SqlServerConfigurationValidatorTests(ITestOutputHelper output)
        : base(output) { }

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

        this.Validate(
            instance: options,
            expectedErrorCount: 1,
            nameof(SqlServerConfiguration.ConnectionString)
        );
    }

    [Fact]
    public void ConnectionStringCannotBeInvalid()
    {
        SqlServerConfiguration options = new("Qwertyuiop");

        this.Validate(
            instance: options,
            expectedErrorCount: 1,
            nameof(SqlServerConfiguration.ConnectionString)
        );
    }
}
