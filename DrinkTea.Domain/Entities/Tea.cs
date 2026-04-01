namespace DrinkTea.Domain.Entities;

/// <summary>
///     Представляет сорт чая в системе.
/// </summary>
/// <remarks>
///     Основная единица учета на складе. Вес хранится в граммах.
/// </remarks>
public class Tea
{
    public Guid Id { get; set; }

    /// <summary>
    ///     Название сорта (например, "Да Хун Пао Премиум").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Текущий остаток на основном складе в граммах.
    /// </summary>
    public decimal CurrentStock { get; set; }

    /// <summary>
    ///     Единица измерения (по умолчанию "g").
    /// </summary>
    public string Unit { get; set; } = "g";
}
