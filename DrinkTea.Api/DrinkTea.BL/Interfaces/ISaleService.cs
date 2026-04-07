using DrinkTea.Shared.Enums;

namespace DrinkTea.BL.Interfaces;

/// <summary>
/// Defines retail sale business operations.
/// </summary>
public interface ISaleService
{
    /// <summary>
    /// Creates a tea sale.
    /// </summary>
    Task<Guid> SellAsync(Guid teaId, decimal grams, PaymentMethod method, Guid staffId, Guid? userId = null);
}
