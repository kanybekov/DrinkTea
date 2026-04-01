using DrinkTea.Api.Models.Requests;
using DrinkTea.BL.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class SaleController(SaleService saleService) : ControllerBase
{
    /// <summary>
    /// 	Провести розничную продажу чая.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MakeSale([FromBody] SaleRequest req)
    {
        var resultId = await saleService.SellAsync(req.TeaId, req.Grams, req.PaymentMethod, req.UserId);
        return Ok(new { SaleId = resultId });
    }
}
