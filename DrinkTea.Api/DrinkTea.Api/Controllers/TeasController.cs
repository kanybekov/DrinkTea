using DrinkTea.Api.Models.Responses;
using DrinkTea.BL.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrinkTea.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeasController(TeaService teaService) : ControllerBase
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
}
