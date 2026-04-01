using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Infrastructure;

/// <summary>
/// 	Компонент для генерации и валидации JWT-токенов доступа.
/// </summary>
public class JwtProvider(IConfiguration config)
{
    /// <summary>
    /// 	Создает подписанный токен на основе данных пользователя.
    /// </summary>
    /// <remarks>
    /// 	В токен "зашиваются" Claims: ID пользователя, его имя и роль.
    /// </remarks>
    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()), // Наш Enum превратится в строку "Master" или "Customer"
			new Claim("login", user.Login)
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(config["Jwt:ExpiryMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
