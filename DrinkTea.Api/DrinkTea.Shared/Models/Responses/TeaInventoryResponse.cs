using System;

namespace DrinkTea.Shared.Models.Responses
{
    /// <summary>
    /// Информация о сорте чая для складского учета.
    /// </summary>
    public class TeaInventoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal BrewPrice { get; set; }
        public decimal SalePrice { get; set; }

        // Пустой конструктор для Blazor
        public TeaInventoryResponse() { }

        // Конструктор для удобства (если API его использует)
        public TeaInventoryResponse(Guid id, string name, decimal currentStock, decimal brewPrice, decimal salePrice)
        {
            Id = id;
            Name = name;
            CurrentStock = currentStock;
            BrewPrice = brewPrice;
            SalePrice = salePrice;
        }
    }
}
