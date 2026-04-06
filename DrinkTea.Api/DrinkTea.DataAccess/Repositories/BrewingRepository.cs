using Dapper;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.DataAccess.Repositories;

/// <summary>
/// 	Реализация репозитория для управления сессиями заваривания через Dapper.
/// </summary>
public class BrewingRepository(DbConnectionFactory db) : IBrewingRepository
{
    public async Task<bool> FinishSessionAsync(Guid sessionId, IDbTransaction transaction)
    {
        const string sql = "UPDATE BrewingSessions SET IsFinished = TRUE WHERE Id = @Id;";
        var rows = await transaction.Connection.ExecuteAsync(sql, new { Id = sessionId }, transaction);
        return rows > 0;
    }

    public async Task<Guid> CreateSessionAsync(Guid teaId, Guid priceId, decimal grams, decimal totalCost, Guid staffId, IDbTransaction transaction)
    {
        const string sql = @"
		    INSERT INTO BrewingSessions (TeaId, PriceSnapshotId, TotalGrams, TotalCost, StaffId)
		    VALUES (@TeaId, @PriceId, @Grams, @TotalCost, @StaffId)
		    RETURNING Id;";

        return await transaction.Connection.QuerySingleAsync<Guid>(sql,
            new { TeaId = teaId, PriceId = priceId, Grams = grams, TotalCost = totalCost, StaffId = staffId },
            transaction);
    }

    public async Task AddParticipantAsync(Guid sessionId, Guid visitId, decimal shareCost, IDbTransaction transaction)
    {
        const string sql = @"
			INSERT INTO BrewingParticipants (SessionId, VisitId, ShareCost)
			VALUES (@SessionId, @VisitId, @ShareCost);";

        await transaction.Connection.ExecuteAsync(sql,
            new { SessionId = sessionId, VisitId = visitId, ShareCost = shareCost },
            transaction);
    }

    public async Task UpdateAllSharesInSessionAsync(Guid sessionId, decimal newShareCost, IDbTransaction transaction)
    {
        const string sql = @"
			UPDATE BrewingParticipants 
			SET ShareCost = @NewShareCost 
			WHERE SessionId = @SessionId;";

        await transaction.Connection.ExecuteAsync(sql,
            new { SessionId = sessionId, NewShareCost = newShareCost },
            transaction);
    }

    public async Task RemoveParticipantAsync(Guid sessionId, Guid visitId, IDbTransaction transaction)
    {
        const string sql = @"
			DELETE FROM BrewingParticipants 
			WHERE SessionId = @SessionId AND VisitId = @VisitId;";

        await transaction.Connection.ExecuteAsync(sql,
            new { SessionId = sessionId, VisitId = visitId },
            transaction);
    }

    public async Task<BrewingSession> GetSessionByIdAsync(Guid sessionId, IDbTransaction transaction)
    {
        const string sql = "SELECT * FROM BrewingSessions WHERE Id = @Id;";
        return await transaction.Connection.QuerySingleAsync<BrewingSession>(sql, new { Id = sessionId }, transaction);
    }

    public async Task<List<BrewingParticipant>> GetParticipantsBySessionIdAsync(Guid sessionId, IDbTransaction transaction)
    {
        const string sql = "SELECT * FROM BrewingParticipants WHERE SessionId = @SessionId;";
        var result = await transaction.Connection.QueryAsync<BrewingParticipant>(sql, new { SessionId = sessionId }, transaction);
        return result.ToList();
    }
    public async Task DeleteSessionAsync(Guid sessionId, IDbTransaction transaction)
    {
        const string sql = "DELETE FROM BrewingSessions WHERE Id = @Id;";
        await transaction.Connection.ExecuteAsync(sql, new { Id = sessionId }, transaction);
    }

    public async Task<IEnumerable<dynamic>> GetActiveSessionsWithParticipantsAsync()
    {
        using var connection = db.CreateConnection();
        const string sql = @"
        SELECT 
            s.Id, 
            t.Name as TeaName, 
            s.TotalGrams as Grams, 
            s.TotalCost as TotalCost, 
            s.CreatedAt as CreatedAt,
            -- Формируем JSON массив из ID визита и Имени (с учетом анонимов)
            json_agg(json_build_object(
                'VisitId', p.VisitId, 
                'Name', COALESCE(u.FullName, v.Note, 'Гость')
            )) as ParticipantsJson
        FROM BrewingSessions s
        JOIN Teas t ON s.TeaId = t.Id
        JOIN BrewingParticipants p ON s.Id = p.SessionId
        JOIN Visits v ON p.VisitId = v.Id
        LEFT JOIN Users u ON v.UserId = u.Id
        WHERE s.IsFinished = FALSE
        GROUP BY s.Id, t.Name, s.TotalGrams, s.TotalCost, s.CreatedAt
        ORDER BY s.CreatedAt DESC;";

        return await connection.QueryAsync(sql);
    }

    public async Task<bool> UpdateParticipantShareAsync(Guid sessionId, Guid visitId, decimal newShare, IDbTransaction transaction)
    {
        const string sql = @"
        UPDATE BrewingParticipants 
        SET ShareCost = @NewShare 
        WHERE SessionId = @SessionId AND VisitId = @VisitId;";

        var rows = await transaction.Connection.ExecuteAsync(sql,
            new { SessionId = sessionId, VisitId = visitId, NewShare = newShare },
            transaction);

        return rows > 0;
    }

}
