using System.Security.Claims;
using DrinkTea.Domain.Enums;

public class UserContext(IHttpContextAccessor httpContextAccessor)
{
    public Guid UserId
    {
        get
        {
            var idClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var guid) ? guid : Guid.Empty;
        }
    }

    /// <summary>
    /// Получает роль текущего пользователя из Claims токена.
    /// </summary>
    public UserRoles Role
    {
        get
        {
            var roleClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value
                            ?? httpContextAccessor.HttpContext?.User.FindFirst("role")?.Value;

            // Пробуем распарсить строку из токена в наш Enum
            return Enum.TryParse<UserRoles>(roleClaim, out var role) ? role : UserRoles.Customer;
        }
    }
}
