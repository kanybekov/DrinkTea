namespace DrinkTea.Shared.Models.Responses;

public class TeaFeedbackResponse
{
    public Guid TeaId { get; set; }
    public string TeaName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int RatingsCount { get; set; }
    public List<PublicReviewDto> PublicReviews { get; set; } = new();
    public List<PrivateNoteDto> PrivateNotes { get; set; } = new();
}

public class PublicReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PrivateNoteDto
{
    public string NoteText { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
