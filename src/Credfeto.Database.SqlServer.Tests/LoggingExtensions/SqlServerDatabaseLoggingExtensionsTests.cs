using System;
using Credfeto.Database.SqlServer.LoggingExtensions;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Credfeto.Database.SqlServer.Tests.LoggingExtensions;

public sealed class SqlServerDatabaseLoggingExtensionsTests : TestBase
{
    [Fact]
    public void LogAndDispatchTransientExceptionDoesNotThrow()
    {
        ILogger<SqlServerDatabase> logger = GetSubstitute<ILogger<SqlServerDatabase>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        logger.LogAndDispatchTransientException(
            typeName: "SqlException",
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
        ILogger<SqlServerDatabase> logger = GetSubstitute<ILogger<SqlServerDatabase>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(true);

        logger.LogAndDispatchTransientException(
            typeName: "SqlException",
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
        ILogger<SqlServerDatabase> logger = GetSubstitute<ILogger<SqlServerDatabase>>();
        logger.IsEnabled(Arg.Any<LogLevel>()).Returns(false);

        logger.LogAndDispatchTransientException(
            typeName: "SqlException",
            retryCount: 1,
            maxRetries: 3,
            delay: TimeSpan.FromSeconds(1),
            details: "some details",
            exception: new InvalidOperationException("test")
        );
    }
}
