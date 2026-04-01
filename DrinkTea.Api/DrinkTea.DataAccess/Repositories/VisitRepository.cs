using Dapper;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.DataAccess.Repositories;

/// <summary>
/// 	Реализация репозитория для управления визитами через Dapper.
/// </summary>
public class VisitRepository(DbConnectionFactory db) : IVisitRepository
{
    public async Task<Visit> CreateAsync(Guid? userId)
    {
        using var connection = db.CreateConnection();

        const string sql = @"
			INSERT INTO Visits (UserId, StartTime, TotalAmount, IsClosed)
			VALUES (@UserId, CURRENT_TIMESTAMP, 0, FALSE)
			RETURNING Id, UserId, StartTime, TotalAmount, IsClosed;";

        return await connection.QuerySingleAsync<Visit>(sql, new { UserId = userId });
    }

    public async Task<Visit?> GetByIdAsync(Guid id)
    {
        using var connection = db.CreateConnection();

        const string sql = "SELECT * FROM Visits WHERE Id = @Id;";

        return await connection.QueryFirstOrDefaultAsync<Visit>(sql, new { Id = id });
    }

    public async Task<bool> AddToTotalAsync(Guid visitId, decimal amount, IDbTransaction transaction)
    {
        // Используем transaction.Connection, так как работаем внутри существующей транзакции BL
        const string sql = @"
			UPDATE Visits 
			SET TotalAmount = TotalAmount + @Amount 
			WHERE Id = @VisitId AND IsClosed = FALSE;";

        var rows = await transaction.Connection.ExecuteAsync(sql,
            new { VisitId = visitId, Amount = amount }, transaction);

        return rows > 0;
    }

    public async Task<bool> CloseAsync(Guid visitId, IDbTransaction transaction)
    {
        const string sql = @"
			UPDATE Visits 
			SET IsClosed = TRUE, EndTime = CURRENT_TIMESTAMP 
			WHERE Id = @VisitId;";

        var rows = await transaction.Connection.ExecuteAsync(sql,
            new { VisitId = visitId }, transaction);

        return rows > 0;
    }
    public async Task<bool> UpdateUserBalanceAsync(Guid userId, decimal amount, IDbTransaction transaction)
    {
        const string sql = "UPDATE Users SET Balance = Balance + @Amount WHERE Id = @UserId;";
        var rows = await transaction.Connection.ExecuteAsync(sql, new { UserId = userId, Amount = amount }, transaction);
        return rows > 0;
    }

    public async Task RegisterTransactionAsync(Transaction tx, IDbTransaction transaction)
    {
        const string sql = @"
		INSERT INTO Transactions (VisitId, UserId, Amount, PaymentMethod)
		VALUES (@VisitId, @UserId, @Amount, @PaymentMethod);";
        await transaction.Connection.ExecuteAsync(sql, tx, transaction);
    }

    public async Task<IEnumerable<dynamic>> GetActiveVisitsWithNamesAsync()
    {
        using var connection = db.CreateConnection();

        const string sql = @"
		SELECT 
			v.Id, 
			v.UserId, 
			v.StartTime, 
			v.TotalAmount, 
			u.FullName as UserName
		FROM Visits v
		LEFT JOIN Users u ON v.UserId = u.Id
		WHERE v.IsClosed = FALSE
		ORDER BY v.StartTime DESC;";

        return await connection.QueryAsync(sql);
    }

    public async Task<IEnumerable<dynamic>> GetVisitItemsAsync(Guid visitId)
    {
        using var connection = db.CreateConnection();

        const string sql = @"
		SELECT 
			s.Id as SessionId,
			t.Name as TeaName,
			s.TotalGrams as Grams,
			p.ShareCost as ShareCost,
			s.CreatedAt as Time
		FROM BrewingParticipants p
		JOIN BrewingSessions s ON p.SessionId = s.Id
		JOIN Teas t ON s.TeaId = t.Id
		WHERE p.VisitId = @VisitId
		ORDER BY s.CreatedAt DESC;";

        return await connection.QueryAsync(sql, new { VisitId = visitId });
    }

    public async Task<bool> HasActiveVisitAsync(Guid userId)
    {
        using var connection = db.CreateConnection();
        const string sql = "SELECT EXISTS(SELECT 1 FROM Visits WHERE UserId = @UserId AND IsClosed = FALSE);";
        return await connection.ExecuteScalarAsync<bool>(sql, new { UserId = userId });
    }


}