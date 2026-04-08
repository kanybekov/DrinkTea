using DrinkTea.BL.Infrastructure;
using DrinkTea.BL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DrinkTea.Shared.Models.Requests;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, JwtProvider jwtProvider) : ControllerBase
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
            var token = jwtProvider.GenerateToken(user);

            return Ok(new DrinkTea.Shared.Models.Responses.LoginResponse
            {
                Token = token,
                FullName = user.FullName,
                Role = user.Role // Теперь роль улетает на фронт
            });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }
}

