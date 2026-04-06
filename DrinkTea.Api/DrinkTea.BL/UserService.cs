using Dapper;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Enums;
using DrinkTea.Shared.Models.Responses;

namespace DrinkTea.BL.Services;

public class UserService(IUserRepository userRepo, IVisitRepository visitRepo, DbConnectionFactory db)
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
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Увеличиваем баланс в таблице Users (теперь передаем транзакцию)
            var success = await visitRepo.UpdateUserBalanceAsync(userId, amount, transaction);
            if (!success) throw new Exception("Пользователь не найден или ошибка обновления.");

            // 2. Фиксируем пополнение в транзакциях
            await visitRepo.RegisterTransactionAsync(new Transaction
            {
                UserId = userId,
                StaffId = staffId,
                Amount = amount,
                PaymentMethod = method,
                Description = $"Пополнение баланса ({method})"
            }, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
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

        return profile;
    }


    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // Можно добавить фильтр, чтобы мастер не видел других мастеров в списке гостей
        // return await userRepo.GetAllAsync(); 
        // Но пока давай просто всех:
        using var connection = db.CreateConnection();
        return await connection.QueryAsync<User>("SELECT * FROM Users ORDER BY FullName");
    }

}
