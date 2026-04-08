using DrinkTea.Shared.Models.Responses;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for report endpoints.
/// </summary>
public interface IReportsApiService
{
    /// <summary>
    /// Gets aggregated daily report.
    /// </summary>
    Task<CashReportResponse?> GetDailyReportAsync(DateTime date);

    /// <summary>
    /// Gets detailed daily report.
    /// </summary>
    Task<List<TransactionDetailResponse>?> GetDetailedReportAsync(DateTime date);
}
