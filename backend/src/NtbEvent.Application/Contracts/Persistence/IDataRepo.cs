using System.Data;

namespace NtbEvent.Application.Contracts.Persistence;

public interface IDataRepo
{
    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default);
}
