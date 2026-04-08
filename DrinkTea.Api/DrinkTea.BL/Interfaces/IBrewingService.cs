using DrinkTea.Shared.Models.Responses;

namespace DrinkTea.BL.Interfaces;

/// <summary>
/// Defines brewing-session business operations.
/// </summary>
public interface IBrewingService
{
    /// <summary>
    /// Starts a new brewing session.
    /// </summary>
    Task<Guid> StartBrewingAsync(Guid teaId, decimal grams, List<Guid> visitIds, Guid userId);

    /// <summary>
    /// Adds a participant to an existing session.
    /// </summary>
    Task JoinSessionAsync(Guid sessionId, Guid visitId);

    /// <summary>
    /// Removes a participant from an existing session.
    /// </summary>
    Task LeaveSessionAsync(Guid sessionId, Guid visitId);

    /// <summary>
    /// Cancels a brewing session.
    /// </summary>
    Task CancelSessionAsync(Guid sessionId);

    /// <summary>
    /// Marks a brewing session as finished.
    /// </summary>
    Task FinishSessionAsync(Guid sessionId);

    /// <summary>
    /// Gets visit item history.
    /// </summary>
    Task<IEnumerable<dynamic>> GetVisitHistoryAsync(Guid visitId);

    /// <summary>
    /// Gets active brewing sessions.
    /// </summary>
    Task<IEnumerable<ActiveBrewingDto>> GetActiveSessionsAsync();
}
