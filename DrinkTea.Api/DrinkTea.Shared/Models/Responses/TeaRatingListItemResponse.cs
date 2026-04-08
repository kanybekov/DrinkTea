namespace DrinkTea.Shared.Models.Responses;

public class TeaRatingListItemResponse
{
    public Guid TeaId { get; set; }
    public string TeaName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int RatingsCount { get; set; }
    public List<PublicReviewDto> RecentPublicComments { get; set; } = new();
}
