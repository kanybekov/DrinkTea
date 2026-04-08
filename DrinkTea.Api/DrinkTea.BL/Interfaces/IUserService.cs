using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Enums;
using DrinkTea.Shared.Models.Responses;

namespace DrinkTea.BL.Interfaces;

/// <summary>
/// Defines user and profile business operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task<Guid> CreateUserAsync(string fullName, string login, string password, UserRoles role);

    /// <summary>
    /// Tops up user balance.
    /// </summary>
    Task TopUpBalanceAsync(Guid userId, decimal amount, PaymentMethod method, Guid staffId);

    /// <summary>
    /// Gets user by id.
    /// </summary>
    Task<User> GetUserAsync(Guid id);

    /// <summary>
    /// Gets user statistics.
    /// </summary>
    Task<dynamic> GetUserStatisticsAsync(Guid userId);

    /// <summary>
    /// Gets user full profile.
    /// </summary>
    Task<CustomerFullProfileResponse> GetUserFullProfileAsync(Guid userId);

    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<IEnumerable<User>> GetAllUsersAsync();
}
