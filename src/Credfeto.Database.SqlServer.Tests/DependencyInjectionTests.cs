using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.SqlServer.Tests;

public sealed class DependencyInjectionTests : DependencyInjectionTestsBase
{
    public DependencyInjectionTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure)
    {
    }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services.AddMockedService<IOptions<SqlServerConfiguration>>(x => x.Value.Returns(new SqlServerConfiguration("Database=Example;Server=.;Integrated Security=SSPI")))
                       .AddSqlServerPostgresql();
    }

    [Fact]
    public void DatabaseMustBeRegistered()
    {
        this.RequireService<IDatabase>();
    }
}