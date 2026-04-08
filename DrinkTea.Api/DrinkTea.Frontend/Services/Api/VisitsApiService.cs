using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of visits API service.
/// </summary>
public class VisitsApiService(HttpClient httpClient) : IVisitsApiService
{
    /// <inheritdoc />
    public Task<List<ActiveVisitDto>?> GetActiveAsync()
        => httpClient.GetFromJsonAsync<List<ActiveVisitDto>>("api/visits/active");

    /// <inheritdoc />
    public Task<HttpResponseMessage> CheckInAsync(CheckInRequest request)
        => httpClient.PostAsJsonAsync("api/visits/checkin", request);

    /// <inheritdoc />
    public Task<HttpResponseMessage> CheckoutAsync(Guid visitId, CheckoutRequest request)
        => httpClient.PostAsJsonAsync($"api/visits/{visitId}/checkout", request);
}
