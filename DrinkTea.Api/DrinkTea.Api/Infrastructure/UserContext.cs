using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace DrinkTea.Api.Infrastructure;

/// <summary>
/// 	Предоставляет информацию о текущем авторизованном пользователе.
/// </summary>
public class UserContext(IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// 	Получает ID текущего пользователя из Claims токена.
    /// </summary>
    public Guid UserId
    {
        get
        {
            var idClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var guid) ? guid : Guid.Empty;
        }
    }
}
