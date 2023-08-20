using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Credfeto.Database.Helpers;
using Polly;
using Polly.Retry;

namespace Credfeto.Database;

public abstract class BaseDatabase : IDatabase
{
    private const int MAX_RETRIES = 3;
    private readonly IAsyncPolicy _retryPolicyAsync;

    protected BaseDatabase()
    {
        this._retryPolicyAsync = this.DefineAsyncPolicy();
    }

    public ValueTask<T> ExecuteAsync<T>(Func<DbConnection, CancellationToken, ValueTask<T>> action, CancellationToken cancellationToken)
    {
        async ValueTask<T> Exec()
        {
            await using (DbConnection connection = await this.GetConnectionAsync(cancellationToken))
            {
                return await action(arg1: connection, arg2: cancellationToken);
            }
        }

        return this.ExecuteWithRetriesAsync(func: Exec, context: "ExecuteAsync");
    }

    public ValueTask ExecuteAsync(Func<DbConnection, CancellationToken, ValueTask> action, CancellationToken cancellationToken)
    {
        async ValueTask Exec()
        {
            await using (DbConnection connection = await this.GetConnectionAsync(cancellationToken))
            {
                await action(arg1: connection, arg2: cancellationToken);
            }
        }

        return this.ExecuteWithRetriesAsync(func: Exec, context: "ExecuteAsync");
    }

    private AsyncRetryPolicy DefineAsyncPolicy()
    {
        return Policy.Handle((Func<Exception, bool>)this.IsTransientException)
                     .WaitAndRetryAsync(retryCount: MAX_RETRIES,
                                        sleepDurationProvider: RetryDelayCalculator.Calculate,
                                        onRetry: (exception, delay, retryCount, context) =>
                                                 {
                                                     this.LogAndDispatchTransientExceptions(exception: exception,
                                                                                            context: context,
                                                                                            delay: delay,
                                                                                            retryCount: retryCount,
                                                                                            maxRetries: MAX_RETRIES);
                                                 });
    }

    protected abstract bool IsTransientException(Exception exception);

    protected abstract void LogAndDispatchTransientExceptions(Exception exception, Context context, in TimeSpan delay, int retryCount, int maxRetries);

    protected abstract ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken);

    private async ValueTask ExecuteWithRetriesAsync(Func<ValueTask> func, string context)
    {
        Context loggingContext = new(context);

        Task Wrapped(Context c)
        {
            return func()
                .AsTask();
        }

        await this._retryPolicyAsync.ExecuteAsync(action: Wrapped, context: loggingContext);
    }

    private async ValueTask<TReturn> ExecuteWithRetriesAsync<TReturn>(Func<ValueTask<TReturn>> func, string context)
    {
        Context loggingContext = new(context);

        Task<TReturn> Wrapped(Context c)
        {
            return func()
                .AsTask();
        }

        return await this._retryPolicyAsync.ExecuteAsync(action: Wrapped, context: loggingContext);
    }
}