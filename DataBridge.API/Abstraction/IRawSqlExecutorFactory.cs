using DataBridge.API.Enum;

namespace DataBridge.API.Abstraction
{
    public interface IRawSqlExecutorFactory
    {
        IRawSqlExecutor Create(DatabaseName dbName);
    }
}
