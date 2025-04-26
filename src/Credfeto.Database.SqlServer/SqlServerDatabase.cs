using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.SqlServer.Extensions;
using Credfeto.Database.SqlServer.LoggingExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Credfeto.Database.SqlServer;

public sealed class SqlServerDatabase : BaseDatabase
{
    private readonly SqlConnectionStringBuilder _connectionStringBuilder;
    private readonly ILogger<SqlServerDatabase> _logger;

    public SqlServerDatabase(IOptions<SqlServerConfiguration> configuration, ILogger<SqlServerDatabase> logger)
    {
        this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        SqlServerConfiguration cfg = configuration.Value ?? throw new ArgumentNullException(nameof(configuration));
        this._connectionStringBuilder = new(cfg.ConnectionString);
    }

    protected override bool IsTransientException(Exception exception)
    {
        if (exception is SqlException sqlException)
        {
            return sqlException.Errors.OfType<SqlError>().Any(IsTransientError);
        }

        return false;
    }

    [SuppressMessage(
        category: "Meziantou.Analyzer",
        checkId: "MA0051: Method is too long",
        Justification = "Needed in this case"
    )]
    private static bool IsTransientError(SqlError err)
    {
        return err.Number switch
        {
            // SQL Error Code: 40501
            // The service is currently busy. Retry the request after 10 seconds. Code: (reason code to be decoded).
            40501 => true,

            // SQL Error Code: 10928
            // Resource ID: %d. The %s limit for the database is %d and has been reached.
            10928 => true,

            // SQL Error Code: 10929
            // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d.
            // However, the server is currently too busy to support requests greater than %d for this database.
            10929 => true,

            // SQL Error Code: 10053
            // A transport-level error has occurred when receiving results from the server.
            // An established connection was aborted by the software in your host machine.
            10053 => true,

            // SQL Error Code: 10054
            // A transport-level error has occurred when sending the request to the server.
            // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
            10054 => true,

            // SQL Error Code: 10060
            // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
            // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
            // is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed
            // because the connected party did not properly respond after a period of time, or established connection failed
            // because connected host has failed to respond.)
            10060 => true,

            // SQL Error Code: 40197
            // The service has encountered an error processing your request. Please try again.
            40197 => true,

            // SQL Error Code: 40540
            // The service has encountered an error processing your request. Please try again.
            40540 => true,

            // SQL Error Code: 40613
            // Database XXXX on server YYYY is not currently available. Please retry the connection later. If the problem persists, contact customer
            // support, and provide them the session tracing ID of ZZZZZ.
            40613 => true,

            // SQL Error Code: 40143
            // The service has encountered an error processing your request. Please try again.
            40143 => true,

            // SQL Error Code 1205
            // Deadlock. Safe to retry
            1205 => true,

            // SQL Error Code: 233
            // The client was unable to establish a connection because of an error during connection initialization process before login.
            // Possible causes include the following: the client tried to connect to an unsupported version of SQL Server; the server was too busy
            // to accept new connections; or there was a resource limitation (insufficient memory or maximum allowed connections) on the server.
            // (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)
            233 => true,

            // SQL Error Code: 64
            // A connection was successfully established with the server, but then an error occurred during the login process.
            // (provider: TCP Provider, error: 0 - The specified network name is no longer available.)
            64 => true,

            // DBNETLIB Error Code: 53
            // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
            53 => true,

            // DBNETLIB Error Code: 20
            // The instance of SQL Server you attempted to connect to does not support encryption.
            20 => true,

            // DBNETLIB Error Code: -2
            // Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.
            -2 => true,
            _ => false,
        };
    }

    protected override void LogAndDispatchTransientExceptions(
        Exception exception,
        Context context,
        in TimeSpan delay,
        int retryCount,
        int maxRetries
    )
    {
        this._logger.LogAndDispatchTransientException(
            typeName: exception.GetType().Name,
            retryCount: retryCount,
            maxRetries: maxRetries,
            delay: delay,
            FormatException(exception: exception, context: context.OperationKey),
            exception: exception
        );
    }

    private static string FormatException(Exception exception, string context)
    {
        int error = 0;

        StringBuilder sb = new StringBuilder().Append("Calling Stored Procedure: ").AppendLine(context).Append(++error);

        if (exception is SqlException sqlException)
        {
            sb = sb.Append("Calling Stored Procedure: ")
                .AppendLine(context)
                .Append(++error)
                .Append(": Error ")
                .Append(sqlException.Number)
                .Append(". Proc: ")
                .Append(sqlException.Procedure)
                .Append(": ")
                .AppendLine(sqlException.Message)
                .AppendErrorsFromException(sqlException: sqlException, initialError: ref error);
        }

        return sb.ToString();
    }

    protected override async ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        SqlConnection connection = new(this._connectionStringBuilder.ConnectionString);

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
