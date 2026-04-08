namespace DrinkTea.Domain.Entities;

public class PrivateNote
{
    public Guid Id { get; set; }
    public Guid TeaId { get; set; }
    public Guid UserId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
