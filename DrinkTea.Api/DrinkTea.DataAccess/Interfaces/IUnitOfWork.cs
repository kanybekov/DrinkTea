using System.Data;

namespace DrinkTea.DataAccess.Interfaces;

/// <summary>
/// Provides transactional boundary for business operations.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
