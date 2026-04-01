namespace DrinkTea.Domain.Entities;

/// <summary>
///     Сессия посещения клуба гостем ("Открытый счет").
/// </summary>
public class Visit
{
    public Guid Id { get; set; }

    /// <summary>
    ///     Ссылка на зарегистрированного пользователя. 
    ///     Если null — визит оформлен на анонимного гостя.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Время начала визита (Check-in).
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Время закрытия счета (Checkout).
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    ///     Текущая накопленная сумма к оплате по данному визиту.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Флаг завершения визита. После закрытия редактирование сумм запрещено.
    /// </summary>
    public bool IsClosed { get; set; }
}
