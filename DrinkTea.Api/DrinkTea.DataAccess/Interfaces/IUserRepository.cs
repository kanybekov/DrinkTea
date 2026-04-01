using DrinkTea.Domain.Entities;

public interface IUserRepository
{
    /// <summary>	Поиск пользователя по логину для проверки пароля. </summary>
    Task<User?> GetByLoginAsync(string login);
}
