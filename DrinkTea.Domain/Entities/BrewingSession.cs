namespace DrinkTea.Domain.Entities;

/// <summary>
/// 	Информация об одной заварке чая.
/// </summary>
public class BrewingSession
{
    public Guid Id { get; set; }
    public Guid TeaId { get; set; }

    /// <summary>
    /// 	Ссылка на снимок цены, который действовал в момент заварки.
    /// </summary>
    public Guid PriceSnapshotId { get; set; }

    public decimal TotalGrams { get; set; }

    /// <summary>
    /// 	Общая стоимость заварки, которая делится на всех участников.
    /// </summary>
    public decimal TotalCost { get; set; }

    public DateTime CreatedAt { get; set; }
}
