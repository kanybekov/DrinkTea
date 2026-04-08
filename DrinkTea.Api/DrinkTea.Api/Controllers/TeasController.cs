using DrinkTea.BL.Interfaces;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrinkTea.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeasController(ITeaService teaService) : ControllerBase
{
    /// <summary>
    /// 	Получить полный список чая с остатками и ценами для инвентаризации.
    /// </summary>
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory()
    {
        var rawData = await teaService.GetFullInventoryAsync();

        // Маппим динамические данные в наш типизированный Response
        var result = rawData.Select(x => new TeaInventoryResponse(
            (Guid)x.id,
            (string)x.name,
            (decimal)x.currentstock,
            (decimal)(x.brewprice ?? 0m), // Если цена не задана, вернем 0
            (decimal)(x.saleprice ?? 0m)
        ));

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTeaRequest req)
    {
        var id = await teaService.CreateTeaWithPriceAsync(req.Name, req.InitialStock, req.BrewPrice, req.SalePrice);
        return Ok(new { TeaId = id });
    }

    [HttpPost("{id:guid}/restock")]
    public async Task<IActionResult> Restock(Guid id, [FromBody] RestockRequest req)
    {
        await teaService.RestockAsync(id, req.Amount, req.NewBrewPrice, req.NewSalePrice);
        return Ok(new { Message = "Склад обновлен" });
    }

    [HttpPatch("{id:guid}/prices")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> UpdatePrices(Guid id, [FromBody] UpdateTeaPricesRequest req)
    {
        // Вызываем метод сервиса (его нужно будет добавить в TeaService)
        await teaService.UpdateTeaPricesAsync(id, req.BrewPrice, req.SalePrice);
        return Ok(new { Message = "Цены обновлены" });
    }
}
