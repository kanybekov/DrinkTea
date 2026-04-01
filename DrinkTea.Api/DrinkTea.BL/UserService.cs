using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Shared.Enums;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Services;

public class UserService(IUserRepository userRepo, IVisitRepository visitRepo)
{
    /// <summary>
    /// 	Регистрирует нового пользователя (клиента или мастера).
    /// </summary>
    public async Task<Guid> CreateUserAsync(string fullName, string login, string password, UserRoles role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Login = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            Balance = 0
        };

        await userRepo.CreateAsync(user);
        return user.Id;
    }

    /// <summary>
    /// 	Пополняет баланс пользователя (депозит).
    /// </summary>
    /// <param name="staffId"> Кто принял деньги. </param>
    public async Task TopUpBalanceAsync(Guid userId, decimal amount, PaymentMethod method, Guid staffId)
    {
        // 1. Увеличиваем баланс в таблице Users
        await visitRepo.UpdateUserBalanceAsync(userId, amount, null); // Передаем null в транзакцию, если работаем без неё здесь

        // 2. Фиксируем пополнение в транзакциях
        await visitRepo.RegisterTransactionAsync(new Transaction
        {
            UserId = userId,
            StaffId = staffId,
            Amount = amount,
            PaymentMethod = method,
            Description = $"Пополнение баланса"
        }, null);
    }

    public async Task<User> GetUserAsync(Guid id)
    {
        return await userRepo.GetByIdAsync(id)
            ?? throw new Exception("Пользователь не найден");
    }

    public async Task<dynamic> GetUserStatisticsAsync(Guid userId)
    {
        var user = await userRepo.GetByIdAsync(userId)
                   ?? throw new Exception("Пользователь не найден");

        var stats = await visitRepo.GetCustomerStatsAsync(userId);

        return stats;
    }

    public async Task<dynamic> GetUserFullProfileAsync(Guid userId)
    {
        var stats = await visitRepo.GetCustomerStatsAsync(userId);
        var history = await visitRepo.GetUserVisitHistoryAsync(userId);

        return new
        {
            Summary = stats,
            RecentVisits = history
        };
    }

}
