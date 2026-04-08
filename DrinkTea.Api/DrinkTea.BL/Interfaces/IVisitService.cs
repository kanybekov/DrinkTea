using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Enums;

namespace DrinkTea.BL.Interfaces;

/// <summary>
/// Defines visit lifecycle business operations.
/// </summary>
public interface IVisitService
{
    /// <summary>
    /// Opens a new visit.
    /// </summary>
    Task<Visit> StartVisitAsync(Guid? userId, string? note);

    /// <summary>
    /// Closes visit with payment details.
    /// </summary>
    Task CheckoutAsync(Guid visitId, decimal internalAmount, decimal externalAmount, PaymentMethod method, Guid staffId);

    /// <summary>
    /// Closes visit and pays by another user's deposit.
    /// </summary>
    Task PayForFriendAsync(Guid payerUserId, Guid targetVisitId, Guid staffId);

    /// <summary>
    /// Returns active dashboard visits.
    /// </summary>
    Task<IEnumerable<dynamic>> GetActiveDashboardAsync();

    /// <summary>
    /// Returns daily payments report data.
    /// </summary>
    Task<IEnumerable<dynamic>> GetRawDailyReportAsync(DateTime date);

    /// <summary>
    /// Returns daily transaction details.
    /// </summary>
    Task<IEnumerable<dynamic>> GetRawDetailedReportAsync(DateTime date);
}
