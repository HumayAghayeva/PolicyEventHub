using DataBridge.API.Abstraction;
using DataBridge.API.Enum;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using DataBridge.API.Utility;
using DataBridge.API.Extensions;

namespace DataBridge.API.Services
{
    public class DapperContext : IRawSqlExecutor
    {
        private readonly string _connectionString;
        private readonly DatabaseName _dbName;

        public DatabaseName DatabaseName => _dbName;
        DatabaseName IRawSqlExecutor.DatabaseName => DatabaseName;

        public DapperContext(IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")
                .TripleDesDecrypt()
                .AddDbNameToConnection("CIBM");

            _dbName = DatabaseName.CIBM;
        }

        public DapperContext(string connectionString, DatabaseName dbName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException("ConnectionString in DapperContext could not be empty!");
            _dbName = dbName;
        }

        #region Instance Methods, this method maintains because of dependency of whole apps.

        public async Task<IEnumerable<TResult>> DefaultQueryAsync<TResult>(string sql, object param = null, IDbTransaction transaction = null)
        {
            using var sqlConnection = new SqlConnection(_connectionString);

            return await sqlConnection.QueryAsync<TResult>(sql, param, transaction);
        }

        public async Task<T> DefaultQuerySingleOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction);
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            var connection = new SqlConnection(_connectionString);

            SqlMapper.SetTypeMap(typeof(T),
                                    new CustomPropertyTypeMap(
                                        typeof(T),
                                        (type, columnName) =>
                                            type.GetProperties().FirstOrDefault(prop =>
                                                prop.GetCustomAttributes(false)
                                                    .OfType<ColumnAttribute>()
                                                    .Any(attr => attr.Name == columnName) || prop.Name == columnName)));
            return await connection.QuerySingleOrDefaultAsync<T>(sql, param, transaction);
        }

        public async Task<TResult> Execute<TResult>(Func<SqlConnection, Task<TResult>> func)
        {
            using var sqlConnection = new SqlConnection(_connectionString);

            return await func(sqlConnection);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            var connection = new SqlConnection(_connectionString);

            SqlMapper.SetTypeMap(typeof(T),
                                    new CustomPropertyTypeMap(
                                        typeof(T),
                                        (type, columnName) =>
                                            type.GetProperties().FirstOrDefault(prop =>
                                                prop.GetCustomAttributes(false)
                                                    .OfType<ColumnAttribute>()
                                                    .Any(attr => attr.Name == columnName) || prop.Name == columnName)));
            return await connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);
        }

        public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            var connection = new SqlConnection(_connectionString);

            SqlMapper.SetTypeMap(typeof(T),
                                    new CustomPropertyTypeMap(
                                        typeof(T),
                                        (type, columnName) =>
                                            type.GetProperties().FirstOrDefault(prop =>
                                                prop.GetCustomAttributes(false)
                                                    .OfType<ColumnAttribute>()
                                                    .Any(attr => attr.Name == columnName) || prop.Name == columnName)));
            return (await connection.QueryAsync<T>(sql, param, transaction)).AsList();
        }

        public async Task<ICollection<T>> QueryCollectionAsync<T>(string sql, object param = null, IDbTransaction transaction = null)
        {
            var connection = new SqlConnection(_connectionString);

            SqlMapper.SetTypeMap(typeof(T),
                                    new CustomPropertyTypeMap(
                                        typeof(T),
                                        (type, columnName) =>
                                            type.GetProperties().FirstOrDefault(prop =>
                                                prop.GetCustomAttributes(false)
                                                    .OfType<ColumnAttribute>()
                                                    .Any(attr => attr.Name == columnName) || prop.Name == columnName)));
            return (await connection.QueryAsync<T>(sql, param, transaction)).AsList();
        }

        #endregion

        public Task<T> FetchSingleAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            SqlMapper.SetTypeMap(typeof(T), new CustomPropertyTypeMap(
                typeof(T),
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name == columnName) || prop.Name == columnName)));

            return connection.QuerySingleOrDefaultAsync<T>(query.QueryText,
                                query.Parameters, query.Transaction, commandType: query.CommandType);
        }

        public Task<T> FetchFirstAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            SqlMapper.SetTypeMap(typeof(T), new CustomPropertyTypeMap(
                typeof(T),
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name == columnName) || prop.Name == columnName)));

            return connection.QueryFirstOrDefaultAsync<T>(query.QueryText, query.Parameters, query.Transaction, commandType: query.CommandType);
        }

        public Task<IEnumerable<T>> FetchAsync<T>(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            var connection = new SqlConnection(_connectionString);
            SqlMapper.SetTypeMap(typeof(T), new CustomPropertyTypeMap(
                typeof(T),
                (type, columnName) =>
                    type.GetProperties().FirstOrDefault(prop =>
                        prop.GetCustomAttributes(false)
                            .OfType<ColumnAttribute>()
                            .Any(attr => attr.Name == columnName) || prop.Name == columnName)));

            return connection.QueryAsync<T>(query.QueryText, query.Parameters, query.Transaction, commandType: query.CommandType);
        }

        public Task<int> ExecuteAsync(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            IDbConnection connection = null;

            if (query.Transaction != null)
                connection = query.Transaction.Connection;
            else
                connection = new SqlConnection(_connectionString);

            return connection.ExecuteAsync(query.QueryText, query.Parameters, query.Transaction, commandType: query.CommandType);
        }

        public async Task<int> ExecuteScalarAsync(QueryDefinition query, CancellationToken cancellationToken = default)
        {
            IDbConnection connection = null;

            if (query.Transaction != null)
                connection = query.Transaction.Connection;
            else
                connection = new SqlConnection(_connectionString);

            var result = await connection.ExecuteScalarAsync<int>(query.QueryText, query.Parameters, query.Transaction, commandType: query.CommandType);

            return result;
        }
    }
}