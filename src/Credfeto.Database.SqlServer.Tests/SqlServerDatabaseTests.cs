using System;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.SqlServer.Tests.Helpers;
using FunFair.Test.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using Xunit;

namespace Credfeto.Database.SqlServer.Tests;

public sealed class SqlServerDatabaseTests : TestBase
{
    private const string VALID_CONNECTION_STRING =
        "Server=localhost,1;Database=test;User Id=sa;Password=test;Encrypt=false;TrustServerCertificate=true;Connection Timeout=1";

    private static SqlServerDatabase CreateInstance()
    {
        IOptions<SqlServerConfiguration> options = GetSubstitute<IOptions<SqlServerConfiguration>>();
        options.Value.Returns(new SqlServerConfiguration(VALID_CONNECTION_STRING));
        ILogger<SqlServerDatabase> logger = NullLogger<SqlServerDatabase>.Instance;

        return new SqlServerDatabase(configuration: options, logger: logger);
    }

    private static SqlException CreateSqlExceptionWithError(int errorNumber)
    {
        SqlError sqlError = SqlExceptionFactory.CreateSqlError(errorNumber);

        return SqlExceptionFactory.CreateSqlException(sqlError);
    }

    private static SqlException CreateSqlExceptionWithNoErrors()
    {
        return SqlExceptionFactory.CreateSqlException();
    }

    private static bool InvokeIsTransientException(SqlServerDatabase instance, Exception exception)
    {
        MethodInfo method =
            typeof(SqlServerDatabase).GetMethod("IsTransientException", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Cannot find IsTransientException method");

        return (bool)(
            method.Invoke(obj: instance, parameters: [exception])
            ?? throw new InvalidOperationException("IsTransientException returned null")
        );
    }

    private static void InvokeLogAndDispatchTransientExceptions(
        SqlServerDatabase instance,
        Exception exception,
        Context context,
        in TimeSpan delay,
        int retryCount,
        int maxRetries
    )
    {
        MethodInfo method =
            typeof(SqlServerDatabase).GetMethod(
                "LogAndDispatchTransientExceptions",
                BindingFlags.Instance | BindingFlags.NonPublic
            ) ?? throw new InvalidOperationException("Cannot find LogAndDispatchTransientExceptions method");

        method.Invoke(obj: instance, parameters: [exception, context, delay, retryCount, maxRetries]);
    }

    [Fact]
    public void Constructor_WithNullConfigurationValue_ThrowsArgumentNullException()
    {
        IOptions<SqlServerConfiguration> options = GetSubstitute<IOptions<SqlServerConfiguration>>();
        ILogger<SqlServerDatabase> logger = NullLogger<SqlServerDatabase>.Instance;

        Assert.Throws<ArgumentNullException>(() => new SqlServerDatabase(configuration: options, logger: logger));
    }

    [Fact]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        SqlServerDatabase instance = CreateInstance();

        Assert.NotNull(instance);
    }

    [Fact]
    public void IsTransientException_WithNonSqlException_ReturnsFalse()
    {
        SqlServerDatabase instance = CreateInstance();
        Exception exception = new InvalidOperationException("not a sql exception");

        bool result = InvokeIsTransientException(instance: instance, exception: exception);

        Assert.False(result, userMessage: "Non-SqlException should not be transient");
    }

    [Fact]
    public void IsTransientException_WithSqlExceptionWithNoErrors_ReturnsFalse()
    {
        SqlServerDatabase instance = CreateInstance();
        SqlException sqlException = CreateSqlExceptionWithNoErrors();

        bool result = InvokeIsTransientException(instance: instance, exception: sqlException);

        Assert.False(result, userMessage: "SqlException with no errors should not be transient");
    }

    [Fact]
    public void IsTransientException_WithSqlExceptionWithNonTransientError_ReturnsFalse()
    {
        SqlServerDatabase instance = CreateInstance();
        SqlException sqlException = CreateSqlExceptionWithError(errorNumber: 50000);

        bool result = InvokeIsTransientException(instance: instance, exception: sqlException);

        Assert.False(result, userMessage: "SqlException with non-transient error code 50000 should not be transient");
    }

    [Theory]
    [InlineData(40501)]
    [InlineData(10928)]
    [InlineData(10929)]
    [InlineData(10053)]
    [InlineData(10054)]
    [InlineData(10060)]
    [InlineData(40197)]
    [InlineData(40540)]
    [InlineData(40613)]
    [InlineData(40143)]
    [InlineData(1205)]
    [InlineData(233)]
    [InlineData(64)]
    [InlineData(53)]
    [InlineData(20)]
    [InlineData(-2)]
    public void IsTransientException_WithTransientSqlErrorCode_ReturnsTrue(int errorNumber)
    {
        SqlServerDatabase instance = CreateInstance();
        SqlException sqlException = CreateSqlExceptionWithError(errorNumber: errorNumber);

        bool result = InvokeIsTransientException(instance: instance, exception: sqlException);

        Assert.True(result, userMessage: $"SqlException with error code {errorNumber} should be transient");
    }

    [Theory]
    [InlineData(40501)]
    [InlineData(10928)]
    [InlineData(10929)]
    [InlineData(10053)]
    [InlineData(10054)]
    [InlineData(10060)]
    [InlineData(40197)]
    [InlineData(40540)]
    [InlineData(40613)]
    [InlineData(40143)]
    [InlineData(1205)]
    [InlineData(233)]
    [InlineData(64)]
    [InlineData(53)]
    [InlineData(20)]
    [InlineData(-2)]
    public void IsTransientError_WithTransientErrorCode_ReturnsTrue(int errorNumber)
    {
        SqlError sqlError = SqlExceptionFactory.CreateSqlError(errorNumber);

        bool result = SqlServerDatabase.IsTransientError(sqlError);

        Assert.True(result, userMessage: $"Error code {errorNumber} should be transient");
    }

    [Theory]
    [InlineData(50000)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(9999)]
    public void IsTransientError_WithNonTransientErrorCode_ReturnsFalse(int errorNumber)
    {
        SqlError sqlError = SqlExceptionFactory.CreateSqlError(errorNumber);

        bool result = SqlServerDatabase.IsTransientError(sqlError);

        Assert.False(result, userMessage: $"Error code {errorNumber} should not be transient");
    }

    [Fact]
    public void FormatException_WithNonSqlException_ReturnsFormattedStringContainingContext()
    {
        InvalidOperationException exception = new("test error message");

        string result = SqlServerDatabase.FormatException(exception: exception, context: "usp_TestProcedure");

        Assert.Contains(
            expectedSubstring: "usp_TestProcedure",
            actualString: result,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void FormatException_WithSqlException_ReturnsFormattedStringContainingContext()
    {
        SqlException sqlException = CreateSqlExceptionWithError(errorNumber: 1234);

        string result = SqlServerDatabase.FormatException(exception: sqlException, context: "usp_TestSqlProc");

        Assert.Contains(
            expectedSubstring: "usp_TestSqlProc",
            actualString: result,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void FormatException_WithSqlExceptionWithNoErrors_ReturnsFormattedStringContainingContext()
    {
        SqlException sqlException = CreateSqlExceptionWithNoErrors();

        string result = SqlServerDatabase.FormatException(exception: sqlException, context: "usp_NoErrors");

        Assert.Contains(
            expectedSubstring: "usp_NoErrors",
            actualString: result,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public Task ExecuteAsync_WithNonExistentServer_ThrowsException()
    {
        SqlServerDatabase instance = CreateInstance();

        return Assert.ThrowsAnyAsync<Exception>(async () =>
            await instance.ExecuteAsync(
                action: (_, _) => ValueTask.CompletedTask,
                cancellationToken: System.Threading.CancellationToken.None
            )
        );
    }

    [Fact]
    public Task ExecuteAsync_Generic_WithNonExistentServer_ThrowsException()
    {
        SqlServerDatabase instance = CreateInstance();

        return Assert.ThrowsAnyAsync<Exception>(async () =>
            await instance.ExecuteAsync<int>(
                action: (_, _) => ValueTask.FromResult(42),
                cancellationToken: System.Threading.CancellationToken.None
            )
        );
    }

    [Fact]
    public Task ExecuteAsync_WithLogger_InvokesLogAndDispatchTransientExceptions()
    {
        IOptions<SqlServerConfiguration> options = GetSubstitute<IOptions<SqlServerConfiguration>>();
        options.Value.Returns(new SqlServerConfiguration(VALID_CONNECTION_STRING));
        ILogger<SqlServerDatabase> logger = this.GetTypedLogger<SqlServerDatabase>();

        SqlServerDatabase instance = new(configuration: options, logger: logger);

        return Assert.ThrowsAnyAsync<Exception>(async () =>
            await instance.ExecuteAsync(
                action: (_, _) => ValueTask.CompletedTask,
                cancellationToken: System.Threading.CancellationToken.None
            )
        );
    }

    [Fact]
    public void LogAndDispatchTransientExceptions_WithNonSqlException_DoesNotThrow()
    {
        SqlServerDatabase instance = CreateInstance();
        InvalidOperationException exception = new("test");
        Context context = new("test-operation");

        InvokeLogAndDispatchTransientExceptions(
            instance: instance,
            exception: exception,
            context: context,
            delay: TimeSpan.FromSeconds(1),
            retryCount: 1,
            maxRetries: 3
        );
    }

    [Fact]
    public void LogAndDispatchTransientExceptions_WithSqlException_DoesNotThrow()
    {
        SqlServerDatabase instance = CreateInstance();
        SqlException sqlException = CreateSqlExceptionWithError(errorNumber: 40501);
        Context context = new("test-operation");

        InvokeLogAndDispatchTransientExceptions(
            instance: instance,
            exception: sqlException,
            context: context,
            delay: TimeSpan.FromSeconds(1),
            retryCount: 1,
            maxRetries: 3
        );
    }
}
