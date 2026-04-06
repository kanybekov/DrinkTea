using System.Text.Json.Serialization;

namespace DrinkTea.Shared.Models.Responses;

public class UserListItemDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }
}
