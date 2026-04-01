namespace DrinkTea.Api.Models.Responses;

/// <summary>
/// 	Информация о сорте чая для складского учета.
/// </summary>
public record TeaInventoryResponse(
    Guid Id,
    string Name,
    decimal CurrentStock,
    decimal BrewPrice,
    decimal SalePrice);
