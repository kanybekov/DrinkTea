using DrinkTea.Shared.Enums;
using System.Collections.Generic;

namespace DrinkTea.Shared.Models.Responses
{
    /// <summary>
    /// Итоговые показатели выручки за период.
    /// </summary>
    public class CashReportResponse
    {
        /// <summary> Общая сумма по конкретному методу оплаты. </summary>
        public Dictionary<PaymentMethod, decimal> Totals { get; set; } = new();

        /// <summary> Общая выручка (сумма всех методов). </summary>
        public decimal GrandTotal { get; set; }

        /// <summary> Количество проведенных операций. </summary>
        public int OperationsCount { get; set; }

        // Пустой конструктор для десериализации в Blazor
        public CashReportResponse() { }

        public CashReportResponse(Dictionary<PaymentMethod, decimal> totals, decimal grandTotal, int operationsCount)
        {
            Totals = totals;
            GrandTotal = grandTotal;
            OperationsCount = operationsCount;
        }
    }
}
