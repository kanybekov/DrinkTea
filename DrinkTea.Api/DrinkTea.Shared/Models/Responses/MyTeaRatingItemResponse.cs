namespace DrinkTea.Shared.Models.Responses;

public class MyTeaRatingItemResponse
{
    public Guid TeaId { get; set; }
    public string TeaName { get; set; } = string.Empty;
    public int? MyRating { get; set; }
    public string? MyComment { get; set; }
    public string? MyPrivateNote { get; set; }
    public DateTime? LastRatedAt { get; set; }
    public DateTime? LastNotedAt { get; set; }
}
