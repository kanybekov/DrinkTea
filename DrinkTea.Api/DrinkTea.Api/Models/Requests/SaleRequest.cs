using DrinkTea.Domain.Common;

namespace DrinkTea.Api.Models.Requests;

/// <summary>
/// 	Данные для оформления розничной продажи чая.
/// </summary>
public record SaleRequest(
    /// <summary>	Какой чай продаем. </summary>
    Guid TeaId,

    /// <summary>	Сколько грамм вешаем. </summary>
    decimal Grams,

    /// <summary>	Способ оплаты (Cash, Card или Internal для депозита). </summary>
    PaymentMethod PaymentMethod,

    /// <summary>	ID клиента (необязательно, если не Internal). </summary>
    Guid? UserId);
