using DrinkTea.Domain.Common;

namespace DrinkTea.Domain.Entities;

/// <summary>
/// 	Запись о движении денежных средств.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public Guid? VisitId { get; set; }
    public Guid? UserId { get; set; }

    /// <summary>
    /// 	Сумма операции.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 	Способ оплаты: Internal (депозит), Cash, Card.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
