using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using ErgoNodeSharp.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Dapper;

namespace ErgoNodeSharp.Data.MsSql
{
    public static class DapperExtensions
    {
        private static readonly ILogger Logger = ApplicationLogging.LoggerFactory?.CreateLogger("DapperExtensions");

        private static readonly IEnumerable<TimeSpan> RetryTimes = new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10)
        };

        private static readonly RetryPolicy RetryPolicy = Policy
            .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
            .Or<TimeoutException>()
            .OrInner<Win32Exception>(SqlServerTransientExceptionDetector.ShouldRetryOn)
            .WaitAndRetry(RetryTimes,
                (exception, timeSpan, retryCount, context) =>
                {
                    Logger?.LogWarning(
                        exception,
                        "Error communicating with database, will retry after {RetryTimeSpan}. Retry attempt {RetryCount}",
                        timeSpan,
                        retryCount
                    );
                });

        private static readonly AsyncRetryPolicy AsyncRetryPolicy = Policy
             .Handle<SqlException>(SqlServerTransientExceptionDetector.ShouldRetryOn)
             .Or<TimeoutException>()
             .OrInner<Win32Exception>(SqlServerTransientExceptionDetector.ShouldRetryOn)
             .WaitAndRetryAsync(RetryTimes,
                (exception, timeSpan, retryCount, context) =>
                {
                    Logger?.LogWarning(
                        exception,
                        "Error communicating with database, will retry after {RetryTimeSpan}. Retry attempt {RetryCount}",
                        timeSpan,
                        retryCount
                    );
                });

        public static int ExecuteWithRetry(this IDbConnection cnn, string sql, object param = null,
                                                        IDbTransaction transaction = null, int? commandTimeout = null,
                                                        CommandType? commandType = null) =>
            RetryPolicy.Execute(() => cnn.Execute(sql, param, transaction, commandTimeout, commandType));

        public static async Task<int> ExecuteAsyncWithRetry(this IDbConnection cnn, string sql, object param = null,
                                                        IDbTransaction transaction = null, int? commandTimeout = null,
                                                        CommandType? commandType = null) =>
            await AsyncRetryPolicy.ExecuteAsync(async () => await cnn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));

        public static T ExecuteScalarWithRetry<T>(this IDbConnection cnn, string sql, object param = null,
                                                IDbTransaction transaction = null, int? commandTimeout = null,
                                                CommandType? commandType = null) =>
            RetryPolicy.Execute(() => cnn.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType));

        public static Task<T> ExecuteScalarAsyncWithRetry<T>(this IDbConnection cnn, string sql, object param = null,
                                                IDbTransaction transaction = null, int? commandTimeout = null,
                                                CommandType? commandType = null) =>
            RetryPolicy.Execute(async () => await cnn.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType));

        public static IEnumerable<T> QueryWithRetry<T>(this IDbConnection cnn, string sql, object param = null,
                                                        IDbTransaction transaction = null, bool buffered = true,
                                                        int? commandTimeout = null, CommandType? commandType = null) =>
            RetryPolicy.Execute(() => cnn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType));

        public static async Task<IEnumerable<T>> QueryAsyncWithRetry<T>(this IDbConnection cnn, string sql, object param = null,
                                                        IDbTransaction transaction = null, int? commandTimeout = null,
                                                        CommandType? commandType = null) =>
            await AsyncRetryPolicy.ExecuteAsync(async () => await cnn.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType));
    }
}
