using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Requests;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of authentication API service.
/// </summary>
public class AuthApiService(HttpClient httpClient) : IAuthApiService
{
    /// <inheritdoc />
    public Task<HttpResponseMessage> LoginAsync(LoginRequest request)
        => httpClient.PostAsJsonAsync("api/auth/login", request);
}
