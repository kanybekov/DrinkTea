using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of brewing API service.
/// </summary>
public class BrewingApiService(HttpClient httpClient) : IBrewingApiService
{
    /// <inheritdoc />
    public Task<List<ActiveBrewingDto>?> GetActiveAsync()
        => httpClient.GetFromJsonAsync<List<ActiveBrewingDto>>("api/brewing/active");

    /// <inheritdoc />
    public Task<List<VisitItemResponse>?> GetByVisitAsync(Guid visitId)
        => httpClient.GetFromJsonAsync<List<VisitItemResponse>>($"api/brewing/by-visit/{visitId}");

    /// <inheritdoc />
    public Task<HttpResponseMessage> StartAsync(StartBrewingDto request)
        => httpClient.PostAsJsonAsync("api/brewing/start", request);

    /// <inheritdoc />
    public Task<HttpResponseMessage> JoinAsync(Guid sessionId, JoinSessionDto request)
        => httpClient.PatchAsJsonAsync($"api/brewing/{sessionId}/join", request);

    /// <inheritdoc />
    public Task<HttpResponseMessage> FinishAsync(Guid sessionId)
        => httpClient.PatchAsync($"api/brewing/{sessionId}/finish", null);

    /// <inheritdoc />
    public Task<HttpResponseMessage> CancelAsync(Guid sessionId)
        => httpClient.DeleteAsync($"api/brewing/{sessionId}");

    /// <inheritdoc />
    public Task<HttpResponseMessage> RemoveParticipantAsync(Guid sessionId, Guid visitId)
        => httpClient.DeleteAsync($"api/brewing/{sessionId}/participants/{visitId}");
}
