using DrinkTea.DataAccess.Interfaces;
using System.Data;

namespace DrinkTea.DataAccess;

/// <summary>
/// Dapper unit of work implementation over a single connection/transaction.
/// </summary>
public sealed class UnitOfWork(DbConnectionFactory dbConnectionFactory) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var connection = dbConnectionFactory.CreateConnection();
        if (connection is not System.Data.Common.DbConnection dbConnection)
        {
            connection.Open();
            return new AppTransaction(connection, connection.BeginTransaction());
        }

        await dbConnection.OpenAsync(cancellationToken);
        var transaction = await dbConnection.BeginTransactionAsync(cancellationToken);
        return new AppTransaction(connection, transaction);
    }

    private sealed class AppTransaction(IDbConnection connection, IDbTransaction transaction) : IAppTransaction
    {
        private bool _completed;

        public IDbTransaction DbTransaction { get; } = transaction;

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            DbTransaction.Commit();
            _completed = true;
            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (!_completed)
            {
                DbTransaction.Rollback();
                _completed = true;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            DbTransaction.Dispose();
            connection.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
