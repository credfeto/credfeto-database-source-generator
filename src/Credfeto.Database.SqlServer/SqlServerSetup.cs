using Microsoft.Extensions.DependencyInjection;

namespace Credfeto.Database.SqlServer;

public static class SqlServerSetup
{
    public static IServiceCollection AddSqlServer(this IServiceCollection services)
    {
        return services.AddSingleton<IDatabase, SqlServerDatabase>();
    }
}