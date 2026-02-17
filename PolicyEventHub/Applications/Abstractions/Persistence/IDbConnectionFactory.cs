using System.Data;

namespace PolicyEventHub.Applications.Abstractions.Persistence
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
