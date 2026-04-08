using System;
using System.Collections.Generic;

namespace DrinkTea.Shared.Models.Responses
{
    public class CustomerFullProfileResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public int VisitsCount { get; set; }
        public string? FavoriteTea { get; set; }

        // Список последних 5 заварок
        public List<LastBrewingDto> RecentBrews { get; set; } = new();
        public List<LastSaleDto> RecentSales { get; set; } = new();

        public CustomerFullProfileResponse() { }
    }

    public class LastBrewingDto
    {
        public Guid TeaId { get; set; }
        public string TeaName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        public LastBrewingDto() { }
    }

    public class LastSaleDto
    {
        public Guid TeaId { get; set; }
        public string TeaName { get; set; } = "";
        public DateTime Date { get; set; }
        public decimal Grams { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
