using DrinkTea.Shared.Enums;
using System;

namespace DrinkTea.Shared.Models.Responses
{
    /// <summary>
    /// Расширенная информация об операции для детального отчета.
    /// </summary>
    public class TransactionDetailResponse
    {
        public Guid Id { get; set; }
        public DateTime Time { get; set; }
        public string? UserName { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? VisitId { get; set; }

        // Пустой конструктор для Blazor
        public TransactionDetailResponse() { }

        // Конструктор для удобства маппинга в контроллере
        public TransactionDetailResponse(Guid id, DateTime time, string? userName, decimal amount, PaymentMethod method, string description, Guid? visitId)
        {
            Id = id;
            Time = time;
            UserName = userName;
            Amount = amount;
            Method = method;
            Description = description;
            VisitId = visitId;
        }
    }
}
