using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for visit endpoints.
/// </summary>
public interface IVisitsApiService
{
    /// <summary>
    /// Gets active visits.
    /// </summary>
    Task<List<ActiveVisitDto>?> GetActiveAsync();

    /// <summary>
    /// Creates a new visit.
    /// </summary>
    Task<HttpResponseMessage> CheckInAsync(CheckInRequest request);

    /// <summary>
    /// Performs visit checkout.
    /// </summary>
    Task<HttpResponseMessage> CheckoutAsync(Guid visitId, CheckoutRequest request);
}
