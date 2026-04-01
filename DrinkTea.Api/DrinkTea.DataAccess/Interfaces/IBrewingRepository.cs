using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.DataAccess.Interfaces;

/// <summary>
/// 	Интерфейс для управления сессиями заваривания (совместными чаепитиями).
/// </summary>
public interface IBrewingRepository
{
    /// <summary>
    /// 	Регистрирует новую сессию заваривания в базе данных.
    /// </summary>
    /// <remarks>
    /// 	Списывает указанный вес чая и фиксирует снимок цены на момент старта.
    /// </remarks>
    /// <param name="teaId">	Идентификатор выбранного сорта чая. </param>
    /// <param name="priceId">	Идентификатор актуальной версии цены из TeaPrices. </param>
    /// <param name="grams">	Вес заварки в граммах. </param>
    /// <param name="totalCost">	Общая стоимость всей сессии. </param>
    /// <param name="transaction">	Активная SQL-транзакция для атомарности операции. </param>
    /// <returns>	Идентификатор созданной сессии (SessionId). </returns>
    Task<Guid> CreateSessionAsync(Guid teaId, Guid priceId, decimal grams, decimal totalCost, Guid staffId, IDbTransaction transaction);

    /// <summary>
    /// 	Добавляет участника в сессию и фиксирует его финансовую долю.
    /// </summary>
    /// <param name="sessionId">	ID сессии заваривания. </param>
    /// <param name="visitId">	ID визита (счета) гостя. </param>
    /// <param name="shareCost">	Рассчитанная сумма доли для этого гостя. </param>
    /// <param name="transaction">	SQL-транзакция. </param>
    Task AddParticipantAsync(Guid sessionId, Guid visitId, decimal shareCost, IDbTransaction transaction);

    /// <summary>
    /// 	Пересчитывает доли всех участников в активной сессии.
    /// </summary>
    /// <remarks>
    /// 	Используется при «подсадке» нового гостя, когда общая стоимость делится на большее число людей.
    /// </remarks>
    /// <param name="sessionId">	ID сессии для пересчета. </param>
    /// <param name="newShareCost">	Новое значение стоимости доли для каждого. </param>
    /// <param name="transaction">	SQL-транзакция. </param>
    Task UpdateAllSharesInSessionAsync(Guid sessionId, decimal newShareCost, IDbTransaction transaction);

    /// <summary>
    /// 	Удаляет участника из сессии (например, при ошибке мастера).
    /// </summary>
    /// <param name="sessionId">	ID сессии. </param>
    /// <param name="visitId">	ID визита участника. </param>
    /// <param name="transaction">	SQL-транзакция. </param>
    Task RemoveParticipantAsync(Guid sessionId, Guid visitId, IDbTransaction transaction);

    /// <summary>
    /// 	Получает данные о сессии заваривания по её ID.
    /// </summary>
    Task<BrewingSession> GetSessionByIdAsync(Guid sessionId, IDbTransaction transaction);

    /// <summary>
    /// 	Получает список всех участников конкретной сессии.
    /// </summary>
    Task<List<BrewingParticipant>> GetParticipantsBySessionIdAsync(Guid sessionId, IDbTransaction transaction);

    /// <summary>
    /// 	Полностью удаляет сессию заваривания из базы данных.
    /// </summary>
    Task DeleteSessionAsync(Guid sessionId, IDbTransaction transaction);
}
