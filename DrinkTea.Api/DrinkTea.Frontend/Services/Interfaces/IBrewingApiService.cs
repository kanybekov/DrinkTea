using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;
using System.Net.Http;

namespace DrinkTea.Frontend.Services.Interfaces;

/// <summary>
/// API contract for brewing endpoints.
/// </summary>
public interface IBrewingApiService
{
    /// <summary>
    /// Gets active brewing sessions.
    /// </summary>
    Task<List<ActiveBrewingDto>?> GetActiveAsync();

    /// <summary>
    /// Gets visit brewing history.
    /// </summary>
    Task<List<VisitItemResponse>?> GetByVisitAsync(Guid visitId);

    /// <summary>
    /// Starts new brewing session.
    /// </summary>
    Task<HttpResponseMessage> StartAsync(StartBrewingDto request);

    /// <summary>
    /// Adds participant to session.
    /// </summary>
    Task<HttpResponseMessage> JoinAsync(Guid sessionId, JoinSessionDto request);

    /// <summary>
    /// Finishes session.
    /// </summary>
    Task<HttpResponseMessage> FinishAsync(Guid sessionId);

    /// <summary>
    /// Cancels session.
    /// </summary>
    Task<HttpResponseMessage> CancelAsync(Guid sessionId);

    /// <summary>
    /// Removes session participant.
    /// </summary>
    Task<HttpResponseMessage> RemoveParticipantAsync(Guid sessionId, Guid visitId);
}
