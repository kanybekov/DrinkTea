namespace DrinkTea.Api.Models.Responses;

/// <summary>
/// 	Строка в чеке гостя (информация о конкретном чаепитии).
/// </summary>
public record VisitItemResponse(
    Guid SessionId,
    string TeaName,
    decimal Grams,
    decimal ShareCost,
    DateTime Time);
