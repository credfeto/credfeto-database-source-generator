using System;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;

namespace Credfeto.Database.Pgsql;

public sealed class PgsqlDatabase : BaseDatabase
{
    private readonly PgsqlServerConfiguration _configuration;
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<PgsqlDatabase> _logger;

    public PgsqlDatabase(IOptions<PgsqlServerConfiguration> configuration, ILogger<PgsqlDatabase> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this._configuration = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        this._dataSource = NpgsqlDataSource.Create(new NpgsqlConnectionStringBuilder(this._configuration.ConnectionString));
    }

    protected override bool IsTransientException(Exception exception)
    {
        return exception is NpgsqlException { IsTransient: true };
    }

    protected override void LogAndDispatchTransientExceptions(Exception exception, Context context, in TimeSpan delay, int retryCount, int maxRetries)
    {
        string details = FormatException(exception: exception, context: context.OperationKey);

        this._logger.LogWarning(new(exception.HResult),
                                exception: exception,
                                $"Retrying transient exception {exception.GetType().Name}, on attempt {retryCount} of {maxRetries}. Current delay is {delay}: {details}");
    }

    private static string FormatException(Exception exception, string context)
    {
        int error = 0;

        StringBuilder sb = new StringBuilder().Append("Calling Stored Procedure: ")
                                              .AppendLine(context)
                                              .Append(++error);

        if (exception is NpgsqlException sqlException)
        {
            sb = sb.Append(": Error ")
                   .Append(sqlException.SqlState)
                   .Append(". Proc: ")
                   .Append(sqlException.BatchCommand)
                   .Append(": ")
                   .AppendLine(sqlException.Message);
        }

        return sb.ToString();
    }

    protected override async ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        return await this._dataSource.OpenConnectionAsync(cancellationToken);
    }
}