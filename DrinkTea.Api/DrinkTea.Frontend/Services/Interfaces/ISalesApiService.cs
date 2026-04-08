using DrinkTea.Shared.Models.Requests;
using System.Net.Http;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for sales endpoints.
/// </summary>
public interface ISalesApiService
{
    /// <summary>
    /// Creates a retail sale.
    /// </summary>
    Task<HttpResponseMessage> CreateSaleAsync(SaleRequest request);
}
