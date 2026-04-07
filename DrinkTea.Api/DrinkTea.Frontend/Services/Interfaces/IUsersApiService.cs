using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for user endpoints.
/// </summary>
public interface IUsersApiService
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<List<UserListItemDto>?> GetAllAsync();

    /// <summary>
    /// Gets current user full profile.
    /// </summary>
    Task<CustomerFullProfileResponse?> GetMyFullProfileAsync();

    /// <summary>
    /// Tops up user balance.
    /// </summary>
    Task<HttpResponseMessage> TopUpAsync(Guid userId, TopUpRequest request);
}
