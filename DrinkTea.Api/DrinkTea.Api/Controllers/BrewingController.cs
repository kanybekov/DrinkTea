using DrinkTea.BL.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BrewingController(BrewingService service) : ControllerBase
{
    /// <summary>
    /// 	Создать новую заварку на компанию.
    /// </summary>
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartBrewingDto dto)
    {
        var sessionId = await service.StartBrewingAsync(dto.TeaId, dto.Grams, dto.VisitIds);
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
}

public record StartBrewingDto(Guid TeaId, decimal Grams, List<Guid> VisitIds);

/// <summary>
/// 	DTO для добавления участника.
/// </summary>
public record JoinSessionDto(Guid VisitId);
