using Microsoft.AspNetCore.Connections;
using PolicyEventHub.Applications.Abstractions.Persistence;

namespace PolicyEventHub.Infrastructure.Persistence.Dapper
{
    public sealed class DapperUnitOfWork : IUnitOfWork
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperUnitOfWork(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken ct)
        {
            using var conn = _connectionFactory.CreateConnection();

            if (conn.State != System.Data.ConnectionState.Open)
                conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                TransactionScopeAccessor.Set(conn, tx);

                await action(ct).ConfigureAwait(false);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
            finally
            {
                TransactionScopeAccessor.Clear();
            }
        }
    }
}