using Dapper;
using Microsoft.Data.Sqlite;

namespace WeldAdminPro.Data.Repositories;

public abstract class RepositoryBase
{
    protected readonly string ConnectionString;

    protected RepositoryBase(string connectionString)
    {
        ConnectionString = connectionString;
    }

    protected SqliteConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    protected async Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null)
    {
        using var connection = CreateConnection();

        return await connection.QueryAsync<T>(
            sql,
            parameters);
    }

    protected async Task<T?> QuerySingleAsync<T>(
        string sql,
        object? parameters = null)
    {
        using var connection = CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<T>(
            sql,
            parameters);
    }

    protected async Task<int> ExecuteAsync(
        string sql,
        object? parameters = null)
    {
        using var connection = CreateConnection();

        return await connection.ExecuteAsync(
            sql,
            parameters);
    }

    protected async Task<T?> ExecuteScalarAsync<T>(
    string sql,
    object? parameters = null)
    {
        using var connection = CreateConnection();

        return await connection.ExecuteScalarAsync<T>(
            sql,
            parameters);
    }
}