using DrinkTea.Api.Infrastructure;
using DrinkTea.Api.Models.Requests;
using DrinkTea.BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master")] // Только персонал может продавать
public class SaleController(SaleService saleService, UserContext userContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> MakeSale([FromBody] SaleRequest req)
    {
        // Берем ID мастера из защищенного контекста (UserContext)
        var currentStaffId = userContext.UserId;

        var resultId = await saleService.SellAsync(
            req.TeaId,
            req.Grams,
            req.PaymentMethod,
            currentStaffId,
            req.UserId);

        return Ok(new { SaleId = resultId });
    }
}

