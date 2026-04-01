namespace DrinkTea.Domain.Entities;

/// <summary>
///     История цен для конкретного сорта чая.
/// </summary>
/// <remarks>
///     Используется для версионирования. Каждая заварка ссылается на конкретную запись цены,
///     чтобы старые отчеты не менялись при обновлении прайса.
/// </remarks>
public class TeaPrice
{
    public Guid Id { get; set; }
    public Guid TeaId { get; set; }

    /// <summary>
    ///     Стоимость 1 грамма при заваривании в клубе.
    /// </summary>
    public decimal BrewPricePerGram { get; set; }

    /// <summary>
    ///     Стоимость 1 грамма (или единицы) при продаже с собой в магазине.
    /// </summary>
    public decimal SalePricePerGram { get; set; }

    /// <summary>
    ///     Дата и время вступления цены в силу.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
