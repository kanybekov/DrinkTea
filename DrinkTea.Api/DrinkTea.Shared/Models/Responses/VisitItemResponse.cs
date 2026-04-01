using System;

namespace DrinkTea.Shared.Models.Responses
{
    /// <summary>
    /// Строка в чеке гостя (информация о конкретном чаепитии).
    /// </summary>
    public class VisitItemResponse
    {
        public Guid SessionId { get; set; }
        public string TeaName { get; set; } = string.Empty;
        public decimal Grams { get; set; }
        public decimal ShareCost { get; set; }
        public DateTime Time { get; set; }

        // Пустой конструктор обязателен для работы Blazor (десериализации JSON)
        public VisitItemResponse() { }

        // Конструктор для удобства использования в контроллерах бэкенда
        public VisitItemResponse(Guid sessionId, string teaName, decimal grams, decimal shareCost, DateTime time)
        {
            SessionId = sessionId;
            TeaName = teaName;
            Grams = grams;
            ShareCost = shareCost;
            Time = time;
        }
    }
}
