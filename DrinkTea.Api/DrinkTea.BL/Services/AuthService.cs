using BCrypt.Net;
using DrinkTea.BL.Interfaces;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис проверки подлинности пользователей.
/// </summary>
public class AuthService(IUserRepository userRepo) : IAuthService
{
    /// <summary>
    /// 	Проверяет пару Логин/Пароль.
    /// </summary>
    /// <returns> Объект пользователя, если данные верны. </returns>
    public async Task<User> AuthenticateAsync(string login, string password)
    {
        var user = await userRepo.GetByLoginAsync(login)
            ?? throw new Exception("Пользователь не найден");

        if (string.IsNullOrEmpty(user.PasswordHash))
            throw new Exception("Ошибка маппинга: Пароль в объекте User пустой!");

        // Убираем возможные пробелы, которые могли прилететь из БД
        bool isValid = BCrypt.Net.BCrypt.Verify(password.Trim(), user.PasswordHash.Trim());

        if (!isValid) throw new Exception("BCrypt: Пароль не подошел к хэшу");

        return user;
    }
}