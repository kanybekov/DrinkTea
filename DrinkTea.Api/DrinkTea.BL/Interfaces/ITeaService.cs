namespace DrinkTea.BL.Interfaces;

/// <summary>
/// Defines tea catalog and inventory business operations.
/// </summary>
public interface ITeaService
{
    /// <summary>
    /// Returns inventory with actual prices.
    /// </summary>
    Task<IEnumerable<dynamic>> GetFullInventoryAsync();

    /// <summary>
    /// Creates tea with initial prices.
    /// </summary>
    Task<Guid> CreateTeaWithPriceAsync(string name, decimal stock, decimal brewPrice, decimal salePrice);

    /// <summary>
    /// Restocks tea and optionally updates prices.
    /// </summary>
    Task RestockAsync(Guid teaId, decimal amount, decimal? newBrewPrice, decimal? newSalePrice);

    /// <summary>
    /// Updates tea prices.
    /// </summary>
    Task UpdateTeaPricesAsync(Guid teaId, decimal brewPrice, decimal salePrice);
}
