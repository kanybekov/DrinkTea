namespace DrinkTea.Shared.Models.Responses;

public class ActiveBrewingDto
{
    public Guid Id { get; set; }
    public string TeaName { get; set; } = "";
    public decimal Grams { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }

    // Вместо List<string> теперь список объектов с ID
    public List<ParticipantDto> Participants { get; set; } = new();
}

public class ParticipantDto
{
    public Guid VisitId { get; set; }
    public string Name { get; set; } = "";
}
