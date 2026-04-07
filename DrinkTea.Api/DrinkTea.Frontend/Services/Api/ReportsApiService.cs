using DrinkTea.Frontend.Services.Interfaces;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http.Json;

namespace DrinkTea.Frontend.Services.Api;

/// <summary>
/// HTTP implementation of reports API service.
/// </summary>
public class ReportsApiService(HttpClient httpClient) : IReportsApiService
{
    /// <inheritdoc />
    public Task<CashReportResponse?> GetDailyReportAsync(DateTime date)
        => httpClient.GetFromJsonAsync<CashReportResponse>($"api/visits/report?date={date:yyyy-MM-dd}");

    /// <inheritdoc />
    public Task<List<TransactionDetailResponse>?> GetDetailedReportAsync(DateTime date)
        => httpClient.GetFromJsonAsync<List<TransactionDetailResponse>>($"api/visits/report/details?date={date:yyyy-MM-dd}");
}
