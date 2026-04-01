namespace DrinkTea.Domain.Common;

/// <summary>
/// 	Доступные способы оплаты в системе.
/// </summary>
public enum PaymentMethod
{
    /// <summary> Списание с личного депозита пользователя. </summary>
    Internal = 1,

    /// <summary> Оплата наличными в кассу. </summary>
    Cash = 2,

    /// <summary> Оплата банковской картой через терминал. </summary>
    Card = 3
}