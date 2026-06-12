using System;
using Credfeto.Database.Pgsql.LoggingExtensions;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Credfeto.Database.Pgsql.Tests.LoggingExtensions;

public sealed class PgSqlDatabaseLoggingExtensionsTests : TestBase
{
    [Fact]
    public void LogAndDispatchTransientExceptionDoesNotThrow()
    {
        ILogger<PgsqlDatabase> logger = GetSubstitute<ILogger<PgsqlDatabase>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        logger.LogAndDispatchTransientException(
            typeName: "NpgsqlException",
            retryCount: 1,
            maxRetries: 3,
            delay: TimeSpan.FromSeconds(1),
            details: "some details",
            exception: new InvalidOperationException("test")
        );
    }

    [Fact]
    public void LogAndDispatchTransientExceptionWithZeroRetryCountDoesNotThrow()
    {
        ILogger<PgsqlDatabase> logger = GetSubstitute<ILogger<PgsqlDatabase>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        logger.LogAndDispatchTransientException(
            typeName: "NpgsqlException",
            retryCount: 0,
            maxRetries: 3,
            delay: TimeSpan.FromMilliseconds(500),
            details: "connection timed out",
            exception: new InvalidOperationException("boom")
        );
    }

    [Fact]
    public void LogAndDispatchTransientExceptionWithDisabledLoggerDoesNotLog()
    {
        ILogger<PgsqlDatabase> logger = GetSubstitute<ILogger<PgsqlDatabase>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(false);

        logger.LogAndDispatchTransientException(
            typeName: "NpgsqlException",
            retryCount: 1,
            maxRetries: 3,
            delay: TimeSpan.FromSeconds(1),
            details: "some details",
            exception: new InvalidOperationException("test")
        );
    }
}
