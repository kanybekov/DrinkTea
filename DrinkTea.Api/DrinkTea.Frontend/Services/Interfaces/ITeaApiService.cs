using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for tea endpoints.
/// </summary>
public interface ITeaApiService
{
    /// <summary>
    /// Gets tea inventory.
    /// </summary>
    Task<List<TeaInventoryResponse>?> GetInventoryAsync();

    /// <summary>
    /// Creates a tea type.
    /// </summary>
    Task<HttpResponseMessage> CreateAsync(CreateTeaRequest request);

    /// <summary>
    /// Restocks selected tea.
    /// </summary>
    Task<HttpResponseMessage> RestockAsync(Guid teaId, RestockRequest request);

    /// <summary>
    /// Updates tea prices.
    /// </summary>
    Task<HttpResponseMessage> UpdatePricesAsync(Guid teaId, decimal brewPrice, decimal salePrice);
}
