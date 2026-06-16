using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using NSubstitute;
using Polly;
using Xunit;

namespace Credfeto.Database.Pgsql.Tests;

public sealed class PgsqlDatabaseTests : TestBase
{
    private const string VALID_CONNECTION_STRING = "Host=127.0.0.1;Port=5433;Database=test;Timeout=1";

    private static PgsqlDatabase CreateInstance()
    {
        IOptions<PgsqlServerConfiguration> options = GetSubstitute<IOptions<PgsqlServerConfiguration>>();
        options.Value.Returns(new PgsqlServerConfiguration(VALID_CONNECTION_STRING));
        ILogger<PgsqlDatabase> logger = NullLogger<PgsqlDatabase>.Instance;

        return new PgsqlDatabase(configuration: options, logger: logger);
    }

    private static bool InvokeIsTransientException(PgsqlDatabase instance, Exception exception)
    {
        MethodInfo method =
            typeof(PgsqlDatabase).GetMethod("IsTransientException", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Cannot find IsTransientException method");

        return (bool)(
            method.Invoke(obj: instance, parameters: [exception])
            ?? throw new InvalidOperationException("IsTransientException returned null")
        );
    }

    private static void InvokeLogAndDispatchTransientExceptions(
        PgsqlDatabase instance,
        Exception exception,
        Context context,
        in TimeSpan delay,
        int retryCount,
        int maxRetries
    )
    {
        MethodInfo method =
            typeof(PgsqlDatabase).GetMethod(
                "LogAndDispatchTransientExceptions",
                BindingFlags.Instance | BindingFlags.NonPublic
            ) ?? throw new InvalidOperationException("Cannot find LogAndDispatchTransientExceptions method");

        method.Invoke(obj: instance, parameters: [exception, context, delay, retryCount, maxRetries]);
    }

    [Fact]
    public void Constructor_WithNullConfigurationValue_ThrowsArgumentNullException()
    {
        IOptions<PgsqlServerConfiguration> options = GetSubstitute<IOptions<PgsqlServerConfiguration>>();
        ILogger<PgsqlDatabase> logger = NullLogger<PgsqlDatabase>.Instance;

        Assert.Throws<ArgumentNullException>(() => new PgsqlDatabase(configuration: options, logger: logger));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        IOptions<PgsqlServerConfiguration> options = GetSubstitute<IOptions<PgsqlServerConfiguration>>();
        options.Value.Returns(new PgsqlServerConfiguration(VALID_CONNECTION_STRING));

        ConstructorInfo constructor =
            typeof(PgsqlDatabase).GetConstructor(
                [typeof(IOptions<PgsqlServerConfiguration>), typeof(ILogger<PgsqlDatabase>)]
            ) ?? throw new InvalidOperationException("Cannot find PgsqlDatabase constructor");

        TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
            constructor.Invoke([options, null])
        );

        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        PgsqlDatabase instance = CreateInstance();

        Assert.NotNull(instance);
    }

    [Fact]
    public void IsTransientException_WithNonNpgsqlException_ReturnsFalse()
    {
        PgsqlDatabase instance = CreateInstance();
        InvalidOperationException exception = new("not an npgsql exception");

        bool result = InvokeIsTransientException(instance: instance, exception: exception);

        Assert.False(result, userMessage: "Non-NpgsqlException should not be transient");
    }

    [Fact]
    public void IsTransientException_WithNonTransientNpgsqlException_ReturnsFalse()
    {
        PgsqlDatabase instance = CreateInstance();
        NpgsqlException npgsqlException = new("non-transient npgsql exception");

        bool result = InvokeIsTransientException(instance: instance, exception: npgsqlException);

        Assert.False(result, userMessage: "NpgsqlException with IsTransient=false should not be transient");
    }

    [Fact]
    public void IsTransientException_WithTransientNpgsqlException_ReturnsTrue()
    {
        PgsqlDatabase instance = CreateInstance();
        TransientNpgsqlException npgsqlException = new();

        bool result = InvokeIsTransientException(instance: instance, exception: npgsqlException);

        Assert.True(result, userMessage: "NpgsqlException with IsTransient=true should be transient");
    }

    [Fact]
    public void FormatException_WithNonNpgsqlException_ReturnsFormattedStringContainingContext()
    {
        InvalidOperationException exception = new("test error message");

        string result = PgsqlDatabase.FormatException(exception: exception, context: "usp_TestProcedure");

        Assert.Contains(
            expectedSubstring: "usp_TestProcedure",
            actualString: result,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void FormatException_WithNpgsqlException_ReturnsFormattedStringContainingContext()
    {
        NpgsqlException npgsqlException = new("test npgsql error");

        string result = PgsqlDatabase.FormatException(exception: npgsqlException, context: "usp_TestNpgsqlProc");

        Assert.Contains(
            expectedSubstring: "usp_TestNpgsqlProc",
            actualString: result,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public void LogAndDispatchTransientExceptions_WithNonNpgsqlException_DoesNotThrow()
    {
        PgsqlDatabase instance = CreateInstance();
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
    public void LogAndDispatchTransientExceptions_WithNpgsqlException_DoesNotThrow()
    {
        PgsqlDatabase instance = CreateInstance();
        NpgsqlException npgsqlException = new("transient error");
        Context context = new("test-operation");

        InvokeLogAndDispatchTransientExceptions(
            instance: instance,
            exception: npgsqlException,
            context: context,
            delay: TimeSpan.FromSeconds(1),
            retryCount: 1,
            maxRetries: 3
        );
    }

    [Fact]
    public Task ExecuteAsync_WithNonExistentServer_ThrowsException()
    {
        PgsqlDatabase instance = CreateInstance();

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
        PgsqlDatabase instance = CreateInstance();

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
        IOptions<PgsqlServerConfiguration> options = GetSubstitute<IOptions<PgsqlServerConfiguration>>();
        options.Value.Returns(new PgsqlServerConfiguration(VALID_CONNECTION_STRING));
        ILogger<PgsqlDatabase> logger = this.GetTypedLogger<PgsqlDatabase>();

        PgsqlDatabase instance = new(configuration: options, logger: logger);

        return Assert.ThrowsAnyAsync<Exception>(async () =>
            await instance.ExecuteAsync(
                action: (_, _) => ValueTask.CompletedTask,
                cancellationToken: System.Threading.CancellationToken.None
            )
        );
    }

    private sealed class TransientNpgsqlException : NpgsqlException
    {
        public TransientNpgsqlException()
            : base("transient test exception") { }

        public TransientNpgsqlException(string message)
            : base(message) { }

        public TransientNpgsqlException(string message, Exception innerException)
            : base(message: message, innerException: innerException) { }

        public override bool IsTransient => true;
    }
}
