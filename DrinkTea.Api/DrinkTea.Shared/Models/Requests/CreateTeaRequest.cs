namespace DrinkTea.Shared.Models.Requests;

/// <summary> Данные для создания нового сорта чая. </summary>
public class CreateTeaRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal InitialStock { get; set; }
    public decimal BrewPrice { get; set; }
    public decimal SalePrice { get; set; }

    // Пустой конструктор для Blazor
    public CreateTeaRequest() { }

    // Конструктор с параметрами (если нужен для API)
    public CreateTeaRequest(string name, decimal initialStock, decimal brewPrice, decimal salePrice)
    {
        Name = name;
        InitialStock = initialStock;
        BrewPrice = brewPrice;
        SalePrice = salePrice;
    }
}
