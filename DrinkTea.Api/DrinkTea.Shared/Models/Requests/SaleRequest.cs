using DrinkTea.Shared.Enums;
using System;

namespace DrinkTea.Shared.Models.Requests
{
    /// <summary>
    /// Данные для оформления розничной продажи чая.
    /// </summary>
    public class SaleRequest
    {
        /// <summary> Какой чай продаем. </summary>
        public Guid TeaId { get; set; }

        /// <summary> Сколько грамм вешаем. </summary>
        public decimal Grams { get; set; }

        /// <summary> Способ оплаты (Cash, Card или Internal для депозита). </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary> ID клиента (необязательно, если не Internal). </summary>
        public Guid? UserId { get; set; }

        // Пустой конструктор для Blazor
        public SaleRequest() { }

        // Конструктор для удобства использования в коде
        public SaleRequest(Guid teaId, decimal grams, PaymentMethod paymentMethod, Guid? userId = null)
        {
            TeaId = teaId;
            Grams = grams;
            PaymentMethod = paymentMethod;
            UserId = userId;
        }
    }
}
