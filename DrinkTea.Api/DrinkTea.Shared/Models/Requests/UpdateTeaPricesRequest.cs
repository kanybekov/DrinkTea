namespace DrinkTea.Shared.Models.Requests
{
    /// <summary>
    /// Запрос на быстрое изменение цен сорта чая.
    /// </summary>
    public class UpdateTeaPricesRequest
    {
        /// <summary> Новая цена за грамм для заварки в зале. </summary>
        public decimal BrewPrice { get; set; }

        /// <summary> Новая цена за грамм для продажи с собой. </summary>
        public decimal SalePrice { get; set; }

        public UpdateTeaPricesRequest() { }

        public UpdateTeaPricesRequest(decimal brewPrice, decimal salePrice)
        {
            BrewPrice = brewPrice;
            SalePrice = salePrice;
        }
    }
}
