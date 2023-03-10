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

    public async Task ExecuteAsync(Func<DbConnection, CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        await using (DbConnection connection = await this.GetConnectionAsync(cancellationToken))
        {
            await this.ExecuteWithRetriesAsync(func: () => action(arg1: connection, arg2: cancellationToken), context: "ExecuteAsync");
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<DbConnection, CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        await using (DbConnection connection = await this.GetConnectionAsync(cancellationToken))
        {
            return await this.ExecuteWithRetriesAsync(func: () => action(arg1: connection, arg2: cancellationToken), context: "ExecuteAsync");
        }
    }

    private AsyncRetryPolicy DefineAsyncPolicy()
    {
        return Policy.Handle((Func<Exception, bool>)this.IsTransientException)
                     .WaitAndRetryAsync(retryCount: MAX_RETRIES,
                                        sleepDurationProvider: RetryDelayCalculator.Calculate,
                                        onRetry: (exception, delay, retryCount, context) =>
                                                 {
                                                     this.LogAndDispatchTransientExceptions(exception: exception, context: context, delay: delay, retryCount: retryCount, maxRetries: MAX_RETRIES);
                                                 });
    }

    protected abstract bool IsTransientException(Exception exception);

    protected abstract void LogAndDispatchTransientExceptions(Exception exception, Context context, in TimeSpan delay, int retryCount, int maxRetries);

    protected abstract ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken);

    private Task ExecuteWithRetriesAsync(Func<Task> func, string context)
    {
        Context loggingContext = new(context);

        Task Wrapped(Context c)
        {
            return func();
        }

        return this._retryPolicyAsync.ExecuteAsync(action: Wrapped, context: loggingContext);
    }

    private async Task<TReturn> ExecuteWithRetriesAsync<TReturn>(Func<Task<TReturn>> func, string context)
    {
        Context loggingContext = new(context);

        Task<TReturn> Wrapped(Context c)
        {
            return func();
        }

        TReturn result = await this._retryPolicyAsync.ExecuteAsync(action: Wrapped, context: loggingContext);

        return result;
    }
}