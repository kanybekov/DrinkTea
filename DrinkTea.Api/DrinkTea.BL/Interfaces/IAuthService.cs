using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Interfaces;

/// <summary>
/// Defines authentication business operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates credentials and returns an authenticated user.
    /// </summary>
    Task<User> AuthenticateAsync(string login, string password);
}
