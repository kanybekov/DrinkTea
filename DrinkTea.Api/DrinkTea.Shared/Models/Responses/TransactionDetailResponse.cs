using DrinkTea.Shared.Enums;

namespace DrinkTea.Api.Models.Responses;

/// <summary>
/// 	Расширенная информация об операции для детального отчета.
/// </summary>
public record TransactionDetailResponse(
    Guid Id,
    DateTime Time,
    string? UserName,
    decimal Amount,
    PaymentMethod Method,
    string Description, // Что именно произошло
    Guid? VisitId);
