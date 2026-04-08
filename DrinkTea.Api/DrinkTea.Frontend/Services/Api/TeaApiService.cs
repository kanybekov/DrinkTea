using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of tea API service.
/// </summary>
public class TeaApiService(HttpClient httpClient) : ITeaApiService
{
    /// <inheritdoc />
    public Task<List<TeaInventoryResponse>?> GetInventoryAsync()
        => httpClient.GetFromJsonAsync<List<TeaInventoryResponse>>("api/teas/inventory");

    /// <inheritdoc />
    public Task<HttpResponseMessage> CreateAsync(CreateTeaRequest request)
        => httpClient.PostAsJsonAsync("api/teas", request);

    /// <inheritdoc />
    public Task<HttpResponseMessage> RestockAsync(Guid teaId, RestockRequest request)
        => httpClient.PostAsJsonAsync($"api/teas/{teaId}/restock", request);

    /// <inheritdoc />
    public Task<HttpResponseMessage> UpdatePricesAsync(Guid teaId, decimal brewPrice, decimal salePrice)
        => httpClient.PatchAsJsonAsync($"api/teas/{teaId}/prices", new { BrewPrice = brewPrice, SalePrice = salePrice });
}
