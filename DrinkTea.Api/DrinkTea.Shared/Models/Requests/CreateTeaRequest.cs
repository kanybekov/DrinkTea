namespace DrinkTea.Api.Models.Requests;

/// <summary>	Данные для создания нового сорта чая. </summary>
public record CreateTeaRequest(
    string Name,
    decimal InitialStock,
    decimal BrewPrice,
    decimal SalePrice);
