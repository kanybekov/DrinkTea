using DrinkTea.Shared.Models.Requests;
using System.Net.Http;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for authentication endpoints.
/// </summary>
public interface IAuthApiService
{
    /// <summary>
    /// Sends login request to API.
    /// </summary>
    Task<HttpResponseMessage> LoginAsync(LoginRequest request);
}
