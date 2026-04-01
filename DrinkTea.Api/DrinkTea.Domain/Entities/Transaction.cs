using DrinkTea.Shared.Enums;   

namespace DrinkTea.Domain.Entities;

/// <summary>
/// 	Запись о движении денежных средств в системе.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public Guid? VisitId { get; set; }
    public Guid? UserId { get; set; }

    /// <summary>	ID мастера, который провёл транзакцию. </summary>
    public Guid StaffId { get; set; }

    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>	Тип или описание операции (например, "Retail", "VisitPayment"). </summary>
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
