using DrinkTea.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;


[ApiController]
[Route("api/[controller]")]
public class BrewingController(IBrewingService service) : ControllerBase
{
    /// <summary>
    /// 	Создать новую заварку на компанию.
    /// </summary>
    [HttpPost("start")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> Start([FromBody] StartBrewingDto dto, [FromServices] UserContext userContext)
    {
        // Мы берем ID мастера НЕ из тела запроса (которое можно подделать),
        // а из защищенного JWT-токена!
        var sessionId = await service.StartBrewingAsync(dto.TeaId, dto.Grams, dto.VisitIds, userContext.UserId);
        return Ok(new { SessionId = sessionId });
    }


    /// <summary>
    /// 	Подсадить гостя в существующую сессию заваривания.
    /// </summary>
    /// <remarks>
    /// 	Автоматически пересчитывает доли всех участников и обновляет их счета.
    /// </remarks>
    [HttpPatch("{sessionId:guid}/join")]
    public async Task<IActionResult> Join(Guid sessionId, [FromBody] JoinSessionDto dto)
    {
        try
        {
            await service.JoinSessionAsync(sessionId, dto.VisitId);
            return Ok(new { Message = "Гость успешно добавлен, доли пересчитаны" });
        }
        catch (Exception ex)
        {
            // В реальном проекте здесь лучше использовать кастомные исключения
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// 	Удалить гостя из сессии заваривания (отмена участия).
    /// </summary>
    [HttpDelete("{sessionId:guid}/participants/{visitId:guid}")]
    public async Task<IActionResult> Leave(Guid sessionId, Guid visitId)
    {
        await service.LeaveSessionAsync(sessionId, visitId);
        return Ok(new { Message = "Участник удален, доли пересчитаны" });
    }

    /// <summary>
    /// 	Полная отмена ошибочной заварки.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await service.CancelSessionAsync(id);
        return Ok(new { Message = "Заварка отменена, чай возвращен на склад, счета очищены" });
    }

    /// <summary>
    /// 	Получить детализацию всех чаепитий для конкретного визита.
    /// </summary>
    /// <remarks>
    /// 	Данные используются для отрисовки списка заварок в карточке гостя.
    /// </remarks>
    /// <param name="visitId">	ID визита (счета). </param>
    [HttpGet("by-visit/{visitId:guid}")]
    public async Task<IActionResult> GetByVisit(Guid visitId)
    {
        // 1. Вызываем метод через сервис бизнес-логики
        var rawItems = await service.GetVisitHistoryAsync(visitId);

        // 2. Маппим результат в нашу Response-модель (record)
        var items = rawItems.Select(x => new VisitItemResponse(
            (Guid)x.sessionid,
            (string)x.teaname,
            (decimal)x.grams,
            (decimal)x.sharecost,
            (DateTime)x.time
        ));

        return Ok(items);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> GetActive()
    {
        var sessions = await service.GetActiveSessionsAsync();
        return Ok(sessions);
    }

    [HttpPatch("{id:guid}/finish")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> Finish(Guid id)
    {
        await service.FinishSessionAsync(id);
        return Ok(new { Message = "Заварка завершена" });
    }
}