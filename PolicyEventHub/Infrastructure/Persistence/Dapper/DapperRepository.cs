using Microsoft.AspNetCore.Connections;
using PolicyEventHub.Applications.Abstractions.Persistence;
using System.Data;

namespace PolicyEventHub.Infrastructure.Persistence.Dapper
{
    public abstract class DapperRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        protected DapperRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        protected IDbConnection CreateOpenConnection()
        {
            var connection = _connectionFactory.CreateConnection();

            if (connection.State != ConnectionState.Open)
                connection.Open();

            return connection;
        }

        protected async Task<TResult?> QuerySingleAsync<TFirst, TSecond, TResult>(
            string sql,
            Func<TFirst, TSecond, TResult> map,
            object? param = null,
            string splitOn = "Id",
            CancellationToken ct = default)
        {
            using var connection = CreateOpenConnection();

            var command = new CommandDefinition(sql, param, cancellationToken: ct);

            var result = await connection
                .QueryAsync(
                    command,
                    map,
                    splitOn: splitOn)
                .ConfigureAwait(false);

            return result.SingleOrDefault();
        }

        protected async Task<T?> QuerySingleAsync<T>(
               string sql,
               object? param = null,
               CancellationToken ct = default)
        {
            using var connection = CreateOpenConnection();

            var command = new CommandDefinition(sql, param, cancellationToken: ct);
            return await connection.QuerySingleOrDefaultAsync<T>(command).ConfigureAwait(false);
        }

        protected async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? param = null,
            CancellationToken ct = default)
        {
            using var connection = CreateOpenConnection();

            var command = new CommandDefinition(sql, param, cancellationToken: ct);
            var result = await connection.QueryAsync<T>(command).ConfigureAwait(false);

            return result.AsList();
        }

        protected async Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            CancellationToken ct = default)
        {
            using var connection = CreateOpenConnection();

            var command = new CommandDefinition(sql, param, cancellationToken: ct);
            return await connection.ExecuteAsync(command).ConfigureAwait(false);
        }

        protected async Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? param = null,
            CancellationToken ct = default)
        {
            using var connection = CreateOpenConnection();

            var command = new CommandDefinition(sql, param, cancellationToken: ct);
            return await connection.ExecuteScalarAsync<T>(command)
                                   .ConfigureAwait(false);
        }
    }
}