namespace DrinkTea.Shared.Models.Requests
{
    public class LoginRequest
    {
        // Пустой конструктор, чтобы Blazor мог создать объект
        public LoginRequest() { }

        // Конструктор для удобства (опционально)
        public LoginRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }

        // ОБЯЗАТЕЛЬНО { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
