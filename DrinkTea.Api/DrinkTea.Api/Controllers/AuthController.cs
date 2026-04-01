using DrinkTea.BL.Infrastructure;
using DrinkTea.BL.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, JwtProvider jwtProvider) : ControllerBase
{
    /// <summary>
    /// 	Вход в систему.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        try
        {
            var user = await authService.AuthenticateAsync(req.Username, req.Password);

            // Генерируем токен (нужно внедрить JwtProvider в контроллер)
            var token = jwtProvider.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                FullName = user.FullName,
                Role = user.Role
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

}

public record LoginRequest(string Username, string Password);
