using DrinkTea.Domain.Entities;

public interface IUserRepository
{
    /// <summary>	Поиск пользователя по логину для проверки пароля. </summary>
    Task<User?> GetByLoginAsync(string login);

    /// <summary> Сохраняет нового пользователя в БД. </summary>
    Task CreateAsync(User user);

    /// <summary> Поиск пользователя по ID (для профиля). </summary>
    Task<User?> GetByIdAsync(Guid id);

    Task UpdateBalanceAsync(Guid userId, decimal amount);

    Task UpdateAsync(User user);
}
