using System.Data;

namespace PolicyEventHub.Infrastructure.Persistence
{
    internal static class TransactionScopeAccessor
    {
        private static readonly AsyncLocal<(IDbConnection Conn, IDbTransaction Tx)?> _current = new();

        public static (IDbConnection Conn, IDbTransaction Tx)? Current => _current.Value;

        public static void Set(IDbConnection conn, IDbTransaction tx)
            => _current.Value = (conn, tx);

        public static void Clear()
            => _current.Value = null;
    }
}
