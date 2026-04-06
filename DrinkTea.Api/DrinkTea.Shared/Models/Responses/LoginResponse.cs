// Вспомогательный класс для чтения ответа API
namespace DrinkTea.Shared.Models.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public string FullName { get; set; } = "";
        public Enums.UserRoles Role { get; set; }
    }
}