
using DrinkTea.Shared.Enums;

namespace DrinkTea.Domain.Entities;

/// <summary>
/// 	Пользователь системы (Мастер или Клиент).
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;

    /// <summary>	Уникальное имя для входа. </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>	Хэшированный пароль (BCrypt). </summary>
    public string? PasswordHash { get; set; }

    /// <summary>	Роль: Master, Customer. </summary>
    public UserRoles Role { get; set; } = UserRoles.Customer;

    /// <summary>	Текущий баланс (депозит). </summary>
    public decimal Balance { get; set; }
}