using DataBridge.API.Enum;
using System.Data;

namespace DataBridge.API.Abstraction
{
   
    
    public sealed record QueryDefinition(string QueryText,object Parameters = null,CommandType CommandType = CommandType.Text,
      IDbTransaction? Transaction = null);

    public interface IRawSqlExecutor
    {
        DatabaseName DatabaseName { get; }
        Task<T> FetchSingleAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default);
        Task<T> FetchFirstAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> FetchAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default);
        Task<int> ExecuteAsync(QueryDefinition query, CancellationToken cancellationToken = default);

        Task<int> ExecuteScalarAsync(QueryDefinition query, CancellationToken cancellationToken = default);
    }
}
