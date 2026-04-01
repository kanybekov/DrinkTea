using DrinkTea.Api.Models.Responses;
using DrinkTea.BL.Services;
using DrinkTea.Domain.Common;
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

    /// <summary>
    /// 	Получить кассовый отчет за конкретный день.
    /// </summary>
    /// <param name="date"> Дата отчета (по умолчанию сегодня). </param>
    [HttpGet("report")]
    public async Task<IActionResult> GetReport([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.Today;
        var rawData = await visitService.GetRawDailyReportAsync(reportDate);

        var totals = new Dictionary<PaymentMethod, decimal>();
        decimal grandTotal = 0;
        int totalOps = 0;

        foreach (var item in rawData)
        {
            var method = (PaymentMethod)Convert.ToInt32(item.method);
            var sum = Convert.ToDecimal(item.total);
            var count = Convert.ToInt32(item.count);

            totals[method] = sum;
            grandTotal += sum;
            totalOps += count;
        }

        return Ok(new CashReportResponse(totals, grandTotal, totalOps));
    }

    /// <summary>
    /// 	Получить подробный список всех транзакций за день.
    /// </summary>
    [HttpGet("report/details")]
    public async Task<IActionResult> GetDetailedReport([FromQuery] DateTime? date)
    {
        var reportDate = date ?? DateTime.Today;
        var rawData = await visitService.GetRawDetailedReportAsync(reportDate);

        var result = rawData.Select(x => new TransactionDetailResponse(
            (Guid)x.id,
            (DateTime)x.time,
            (string?)x.username,
            (decimal)x.amount,
            (PaymentMethod)Convert.ToInt32(x.method),
            (string)x.description, // Данные из CASE в SQL
            (Guid?)x.visitid
        ));

        return Ok(result);
    }
}

public record CheckoutRequest(decimal InternalAmount, decimal ExternalAmount, PaymentMethod Method);
