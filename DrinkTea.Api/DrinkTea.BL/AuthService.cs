using BCrypt.Net;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис проверки подлинности пользователей.
/// </summary>
public class AuthService(IUserRepository userRepo)
{
    /// <summary>
    /// 	Проверяет пару Логин/Пароль.
    /// </summary>
    /// <returns> Объект пользователя, если данные верны. </returns>
    public async Task<User> AuthenticateAsync(string login, string password)
    {
        // 1. Ищем человека в базе
        var user = await userRepo.GetByLoginAsync(login)
            ?? throw new Exception("Пользователь не найден");

        // 2. Проверяем, есть ли у него вообще пароль
        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new Exception("Для этого аккаунта не задан пароль");

        // 3. Сверяем введенный пароль с хэшем в базе
        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!isValid) throw new Exception("Неверный пароль");

        return user;
    }
}