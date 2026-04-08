namespace DrinkTea.Domain.Entities;

public class PublicReview
{
    public Guid Id { get; set; }
    public Guid TeaId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
