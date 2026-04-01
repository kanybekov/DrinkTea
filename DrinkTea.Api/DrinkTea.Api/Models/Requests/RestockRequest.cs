namespace DrinkTea.Api.Models.Requests;

/// <summary>	Данные для прихода (пополнения) товара. </summary>
public record RestockRequest(
    decimal Amount,
    /// <summary> Опционально: новая цена, если закупка изменилась. </summary>
    decimal? NewBrewPrice = null,
    decimal? NewSalePrice = null);