using System.Data;

namespace DrinkTea.DataAccess.Interfaces;

/// <summary>
/// Wraps an active SQL transaction with commit/rollback operations.
/// </summary>
public interface IAppTransaction : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the underlying ADO.NET transaction.
    /// </summary>
    IDbTransaction DbTransaction { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
