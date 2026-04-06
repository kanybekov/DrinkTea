namespace DrinkTea.Shared.Models.Responses;

public class ActiveVisitDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } 
    public string? UserName { get; set; }
    public string? Note { get; set; }
    public decimal UnpaidDebt { get; set; }
    public decimal? UserDeposit { get; set; }
    public DateTime StartTime { get; set; }
}
