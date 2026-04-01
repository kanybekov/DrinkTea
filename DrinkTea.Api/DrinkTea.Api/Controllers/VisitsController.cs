using DrinkTea.Api.Models.Responses;
using DrinkTea.BL.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class VisitsController(VisitService visitService) : ControllerBase
{
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] Guid? userId)
    {
        try
        {
            var visit = await visitService.StartVisitAsync(userId);
            return Ok(visit);
        }
        catch (InvalidOperationException ex)
        {
            // Возвращаем 400 Bad Request с понятным описанием для мастера
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> Checkout(Guid id, [FromBody] CheckoutRequest req)
    {
        await visitService.CheckoutAsync(id, req.InternalAmount, req.ExternalAmount, req.Method);
        return Ok(new { Status = "Closed" });
    }

    /// <summary>
    /// 	Получить список активных визитов для панели мониторинга.
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var activeVisits = await visitService.GetActiveDashboardAsync();
        return Ok(activeVisits);
    }
}

public record CheckoutRequest(decimal InternalAmount, decimal ExternalAmount, string Method);
