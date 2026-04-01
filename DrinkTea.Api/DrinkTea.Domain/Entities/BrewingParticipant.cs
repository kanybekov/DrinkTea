namespace DrinkTea.Domain.Entities;

/// <summary>
/// 	Связь между конкретной заваркой и визитом гостя.
/// </summary>
public class BrewingParticipant
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid VisitId { get; set; }

    /// <summary>
    /// 	Доля стоимости этой заварки, начисленная данному гостю.
    /// </summary>
    public decimal ShareCost { get; set; }
}
