using DrinkTea.Domain.Common;

namespace DrinkTea.Api.Models.Responses;

/// <summary>
/// 	Итоговые показатели выручки за период.
/// </summary>
public record CashReportResponse(
    /// <summary>	Общая сумма по конкретному методу оплаты. </summary>
    Dictionary<PaymentMethod, decimal> Totals,

    /// <summary>	Общая выручка (сумма всех методов). </summary>
    decimal GrandTotal,

    /// <summary>	Количество проведенных операций. </summary>
    int OperationsCount);
