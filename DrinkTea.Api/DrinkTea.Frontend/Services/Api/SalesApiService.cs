using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Requests;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of sales API service.
/// </summary>
public class SalesApiService(HttpClient httpClient) : ISalesApiService
{
    /// <inheritdoc />
    public Task<HttpResponseMessage> CreateSaleAsync(SaleRequest request)
        => httpClient.PostAsJsonAsync("api/sales", request);
}
