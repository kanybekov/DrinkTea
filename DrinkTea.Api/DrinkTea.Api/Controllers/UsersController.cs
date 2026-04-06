using DrinkTea.BL.Services;
using DrinkTea.Shared.Enums;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrinkTea.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserService userService, UserContext userContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest req)
    {
        var id = await userService.CreateUserAsync(req.FullName, req.Login, req.Password, req.Role);
        return Ok(new { UserId = id });
    }

    [HttpPost("{id:guid}/topup")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> TopUp(Guid id, [FromBody] TopUpRequest req)
    {
        await userService.TopUpBalanceAsync(id, req.Amount, req.Method, userContext.UserId);
        return Ok(new { Message = "Баланс успешно пополнен" });
    }

    /// <summary>
    ///     Просмотр данных любого пользователя (для мастера).
    /// </summary>
    [HttpGet("{id:guid}")]
    // Снимаем ограничение "только мастер", чтобы обычный юзер тоже мог вызвать метод
    [Authorize]
    public async Task<IActionResult> Get(Guid id)
    {
        // Проверка доступа: 
        // Если ты не Мастер И пытаешься посмотреть не свой ID -> Отказ
        if (userContext.Role != UserRoles.Master && userContext.UserId != id)
        {
            return Forbid();
        }

        var user = await userService.GetUserAsync(id);

        // Возвращаем DTO без лишних данных (без пароля)
        return Ok(new
        {
            user.Id,
            user.FullName,
            user.Login,
            user.Role,
            user.Balance
        });
    }

    [HttpGet("{id:guid}/stats")]
    [Authorize]
    public async Task<IActionResult> GetStats(Guid id)
    {
        // Безопасность: Клиент не может смотреть чужую статистику
        if (userContext.Role != UserRoles.Master && userContext.UserId != id)
        {
            return Forbid();
        }

        var stats = await userService.GetUserStatisticsAsync(id);
        return Ok(stats);
    }

    [HttpGet("{id:guid}/full-profile")]
    [Authorize]
    public async Task<ActionResult<CustomerFullProfileResponse>> GetFullProfile(Guid id)
    {
        if (userContext.Role != UserRoles.Master && userContext.UserId != id)
            return Forbid();

        var profile = await userService.GetUserFullProfileAsync(id);
        return Ok(profile);
    }

    [HttpGet("me/full-profile")]
    [Authorize]
    public async Task<IActionResult> GetMyFullProfile([FromServices] UserContext userContext)
    {
        // UserContext сам достанет ID из JWT
        var profile = await userService.GetUserFullProfileAsync(userContext.UserId);
        return Ok(profile);
    }

    [HttpGet]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> GetAll()
    {
        // Нам нужен метод в UserService, который вернет всех (или только клиентов)
        var users = await userService.GetAllUsersAsync();

        // Возвращаем список DTO
        return Ok(users.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Balance
        }));
    }
}

