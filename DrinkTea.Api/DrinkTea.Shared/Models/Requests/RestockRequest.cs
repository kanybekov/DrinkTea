namespace DrinkTea.Shared.Models.Requests;

/// <summary> Данные для прихода (пополнения) товара. </summary>
public class RestockRequest
{
    public decimal Amount { get; set; }

    /// <summary> Опционально: новая цена заварки. </summary>
    public decimal? NewBrewPrice { get; set; }

    /// <summary> Опционально: новая цена продажи. </summary>
    public decimal? NewSalePrice { get; set; }

    // Пустой конструктор для Blazor и сериализации
    public RestockRequest() { }

    // Конструктор для удобства (по желанию)
    public RestockRequest(decimal amount, decimal? newBrewPrice = null, decimal? newSalePrice = null)
    {
        Amount = amount;
        NewBrewPrice = newBrewPrice;
        NewSalePrice = newSalePrice;
    }
}
