using DrinkTea.Api.Infrastructure;
using DrinkTea.Api.Models.Responses;
using DrinkTea.BL.Services;
using DrinkTea.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class VisitsController(VisitService visitService) : ControllerBase
{
    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest req)
    {
        try
        {
            var visit = await visitService.StartVisitAsync(req.UserId, req.Note);
            return Ok(visit);
        }
        catch (InvalidOperationException ex)
        {
            // Возвращаем 400 Bad Request с понятным описанием для мастера
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// 	Закрыть счет гостя с проведением оплаты.
    /// </summary>
    /// <remarks>
    /// 	ID мастера автоматически извлекается из JWT-токена через UserContext.
    /// </remarks>
    [HttpPost("{id:guid}/checkout")]
    [Authorize(Roles = "Master")] // Только авторизованный мастер может закрыть чек
    public async Task<IActionResult> Checkout(
        Guid id,
        [FromBody] CheckoutRequest req,
        [FromServices] UserContext userContext) // Внедряем контекст прямо в метод
    {
        // 1. Извлекаем ID мастера из токена
        var staffId = userContext.UserId;

        if (staffId == Guid.Empty)
            return Unauthorized("Не удалось определить личность мастера из токена");

        // 2. Передаем все данные в сервис, включая StaffId
        await visitService.CheckoutAsync(
            id,
            req.InternalAmount,
            req.ExternalAmount,
            req.Method,
            staffId);

        return Ok(new { Status = "Closed", StaffId = staffId });
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

    /// <summary>
    /// 	Закрыть счет гостя, списав сумму с депозита другого пользователя (друга).
    /// </summary>
    [HttpPost("{visitId:guid}/pay-by/{payerUserId:guid}")]
    public async Task<IActionResult> PayForFriend(Guid visitId, Guid payerUserId)
    {
        await visitService.PayForFriendAsync(payerUserId, visitId);
        return Ok(new { Message = "Счет друга успешно оплачен с вашего депозита" });
    }
}

public record CheckInRequest(Guid? UserId, string? Note);

public record CheckoutRequest(decimal InternalAmount, decimal ExternalAmount, PaymentMethod Method);
