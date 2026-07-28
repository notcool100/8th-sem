using System.Data;
using Dapper;
using NtbEvent.Application.Contracts.Persistence;
using NtbEvent.Infrastructure.Configuration;
using Npgsql;

namespace NtbEvent.Infrastructure.Persistence;

public sealed class DataRepo : IDataRepo
{
    private readonly DatabaseOptions _databaseOptions;

    public DataRepo(DatabaseOptions databaseOptions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseOptions.ConnectionString);
        _databaseOptions = databaseOptions;
    }

    public Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(
            transaction,
            async (connection, currentTransaction) =>
            {
                var command = new CommandDefinition(
                    sql,
                    parameters,
                    currentTransaction,
                    commandType: commandType,
                    cancellationToken: cancellationToken);

                return await connection.ExecuteAsync(command);
            },
            cancellationToken);
    }

    public Task<T> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync<T>(
            transaction,
            async (connection, currentTransaction) =>
            {
                var command = new CommandDefinition(
                    sql,
                    parameters,
                    currentTransaction,
                    commandType: commandType,
                    cancellationToken: cancellationToken);

                var result = await connection.ExecuteScalarAsync<T>(command);
                return result!;
            },
            cancellationToken);
    }

    public Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync<IReadOnlyList<T>>(
            transaction,
            async (connection, currentTransaction) =>
            {
                var command = new CommandDefinition(
                    sql,
                    parameters,
                    currentTransaction,
                    commandType: commandType,
                    cancellationToken: cancellationToken);

                var results = await connection.QueryAsync<T>(command);
                return (IReadOnlyList<T>)results.ToList();
            },
            cancellationToken);
    }

    public Task<T?> QuerySingleOrDefaultAsync<T>(
        string sql,
        object? parameters = null,
        IDbTransaction? transaction = null,
        CommandType commandType = CommandType.Text,
        CancellationToken cancellationToken = default)
    {
        return WithConnectionAsync(
            transaction,
            async (connection, currentTransaction) =>
            {
                var command = new CommandDefinition(
                    sql,
                    parameters,
                    currentTransaction,
                    commandType: commandType,
                    cancellationToken: cancellationToken);

                return await connection.QuerySingleOrDefaultAsync<T>(command);
            },
            cancellationToken);
    }

    private async Task<TResult> WithConnectionAsync<TResult>(
        IDbTransaction? transaction,
        Func<IDbConnection, IDbTransaction?, Task<TResult>> action,
        CancellationToken cancellationToken)
    {
        if (transaction?.Connection is not null)
        {
            return await action(transaction.Connection, transaction);
        }

        await using var connection = new NpgsqlConnection(_databaseOptions.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return await action(connection, null);
    }
}
