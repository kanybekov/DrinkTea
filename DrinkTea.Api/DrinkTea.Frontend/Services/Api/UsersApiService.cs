using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of users API service.
/// </summary>
public class UsersApiService(HttpClient httpClient) : IUsersApiService
{
    /// <inheritdoc />
    public Task<List<UserListItemDto>?> GetAllAsync()
        => httpClient.GetFromJsonAsync<List<UserListItemDto>>("api/users");

    /// <inheritdoc />
    public Task<CustomerFullProfileResponse?> GetMyFullProfileAsync()
        => httpClient.GetFromJsonAsync<CustomerFullProfileResponse>("api/users/me/full-profile");

    /// <inheritdoc />
    public Task<HttpResponseMessage> TopUpAsync(Guid userId, TopUpRequest request)
        => httpClient.PostAsJsonAsync($"api/users/{userId}/topup", request);
}
