namespace DrinkTea.Shared.Models.Responses;

public class UserListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public decimal Balance { get; set; }
}
