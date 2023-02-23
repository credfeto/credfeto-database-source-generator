using FunFair.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Credfeto.Database.Pgsql.Tests;

public sealed class DependencyInjectionTests : DependencyInjectionTestsBase
{
    public DependencyInjectionTests(ITestOutputHelper output)
        : base(output: output, dependencyInjectionRegistration: Configure)
    {
    }

    private static IServiceCollection Configure(IServiceCollection services)
    {
        return services.AddMockedService<IOptions<PgsqlServerConfiguration>>(x => x.Value.Returns(new PgsqlServerConfiguration("Host=localhost;Database=test")))
                       .AddPostgresql();
    }

    [Fact]
    public void DatabaseMustBeRegistered()
    {
        this.RequireService<IDatabase>();
    }
}