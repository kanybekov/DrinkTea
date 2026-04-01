namespace DrinkTea.Shared.Models.Responses;

public class ActiveBrewingDto
{
    public Guid Id { get; set; }
    public string TeaName { get; set; } = "";
    public decimal Grams { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> ParticipantNames { get; set; } = new();
}
