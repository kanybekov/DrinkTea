namespace DrinkTea.Api.Models.Responses;

/// <summary>
/// 	Полная информация о клиенте для личного кабинета.
/// </summary>
public record CustomerFullProfileResponse(
    Guid Id,
    string Name,
    decimal Balance,
    /// <summary> Общее количество посещений клуба. </summary>
    int VisitsCount,
    /// <summary> Любимый чай (который заказывал чаще всего). </summary>
    string? FavoriteTea,
    /// <summary> Список последних 5 заварок. </summary>
    List<LastBrewingDto> RecentBrews);

public record LastBrewingDto(string TeaName, DateTime Date, decimal Amount);
