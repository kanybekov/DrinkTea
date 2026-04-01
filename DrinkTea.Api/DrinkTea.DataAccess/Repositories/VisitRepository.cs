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

}