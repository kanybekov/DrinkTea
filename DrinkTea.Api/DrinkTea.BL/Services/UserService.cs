using DrinkTea.BL.Interfaces;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Enums;
using DrinkTea.Shared.Models.Responses;

namespace DrinkTea.BL.Services;

public class UserService(IUserRepository userRepo, IVisitRepository visitRepo, IUnitOfWork unitOfWork) : IUserService
{
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
    /// 	Пополняет баланс пользователя (депозит) внутри транзакции.
    /// </summary>
    public async Task TopUpBalanceAsync(Guid userId, decimal amount, PaymentMethod method, Guid staffId)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Увеличиваем баланс в таблице Users (теперь передаем транзакцию)
            var success = await visitRepo.UpdateUserBalanceAsync(userId, amount, transaction.DbTransaction);
            if (!success) throw new Exception("Пользователь не найден или ошибка обновления.");

            // 2. Фиксируем пополнение в транзакциях
            await visitRepo.RegisterTransactionAsync(new Transaction
            {
                UserId = userId,
                StaffId = staffId,
                Amount = amount,
                PaymentMethod = method,
                Description = $"Пополнение баланса ({method})"
            }, transaction.DbTransaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<User> GetUserAsync(Guid id)
    {
        return await userRepo.GetByIdAsync(id) ?? throw new Exception("Пользователь не найден");
    }

    public async Task<dynamic> GetUserStatisticsAsync(Guid userId)
    {
        return await visitRepo.GetCustomerStatsAsync(userId) ?? throw new Exception("Статистика не найдена");
    }

    public async Task<CustomerFullProfileResponse> GetUserFullProfileAsync(Guid userId)
    {
        var profile = await visitRepo.GetCustomerStatsAsync(userId)
                      ?? throw new Exception("Пользователь не найден");

        profile.RecentBrews = await visitRepo.GetUserVisitHistoryAsync(userId);
        profile.RecentSales = await visitRepo.GetUserSalesHistoryAsync(userId);

        return profile;
    }


    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepo.GetAllAsync();
    }


}
