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
    /// <summary>
    /// 	Создает новую запись о визите.
    /// </summary>
    /// <param name="userId"> ID пользователя или null. </param>
    /// <param name="note"> Заметка мастера для идентификации (например, "Стол 5"). </param>
    public async Task<Visit> CreateAsync(Guid? userId, string? note)
    {
        using var connection = db.CreateConnection();

        const string sql = @"
		INSERT INTO Visits (UserId, Note, StartTime, TotalAmount, IsClosed)
		VALUES (@UserId, @Note, CURRENT_TIMESTAMP, 0, FALSE)
		RETURNING Id, UserId, Note, StartTime, TotalAmount, IsClosed;";

        // Dapper сопоставит @Note с параметром метода note
        return await connection.QuerySingleAsync<Visit>(sql, new { UserId = userId, Note = note });
    }


    public async Task<Visit?> GetByIdAsync(Guid id)
    {
        using var connection = db.CreateConnection();

        const string sql = "SELECT Id, UserId, StartTime, EndTime, TotalAmount, IsClosed, Note FROM Visits WHERE Id = @Id;";

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
        // 1. ПРОВЕРКА: Если в репозиторий пришли нули - это авария
        if (tx.StaffId == Guid.Empty)
        {
            throw new Exception("Критическая ошибка: Попытка записать транзакцию без ID мастера (StaffId is Empty).");
        }

        const string sql = @"
		INSERT INTO Transactions (Id, VisitId, UserId, StaffId, Amount, PaymentMethod, Description)
		VALUES (@Id, @VisitId, @UserId, @StaffId, @Amount, @PaymentMethod, @Description);";

        // 2. Явно указываем параметры, чтобы Dapper не ошибся с именами свойств
        await transaction.Connection.ExecuteAsync(sql, new
        {
            Id = tx.Id == Guid.Empty ? Guid.NewGuid() : tx.Id,
            // Если UserId/VisitId пустые GUID, превращаем их в NULL для базы
            VisitId = tx.VisitId == Guid.Empty ? null : tx.VisitId,
            UserId = tx.UserId == Guid.Empty ? null : tx.UserId,
            StaffId = tx.StaffId, // Сюда ДОЛЖЕН прийти {853c5d7c...}
            Amount = tx.Amount,
            PaymentMethod = (int)tx.PaymentMethod,
            Description = tx.Description
        }, transaction);
    }


    public async Task<IEnumerable<dynamic>> GetActiveVisitsWithNamesAsync()
    {
        using var connection = db.CreateConnection();

        const string sql = @"
		SELECT 
			v.Id, 
			v.UserId, 
			v.StartTime, 
		    v.TotalAmount as UnpaidDebt, 
		    u.Balance as UserDeposit,    
			u.FullName as UserName,
		    v.Note
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

    public async Task<IEnumerable<dynamic>> GetPaymentsSummaryAsync(DateTime from, DateTime to)
    {
        using var connection = db.CreateConnection();

        const string sql = @"
		SELECT 
			PaymentMethod as Method, 
			SUM(Amount) as Total,
			COUNT(*) as Count
		FROM Transactions
		WHERE CreatedAt BETWEEN @From AND @To
		GROUP BY PaymentMethod;";

        return await connection.QueryAsync(sql, new { From = from, To = to });
    }

    public async Task<IEnumerable<dynamic>> GetDetailedTransactionsAsync(DateTime from, DateTime to)
    {
        using var connection = db.CreateConnection();

        const string sql = @"
		SELECT 
			t.Id, 
			t.CreatedAt as Time, 
			u.FullName as UserName, 
			t.Amount, 
			t.PaymentMethod as Method, 
			t.VisitId,
            COALESCE(v.Note, 'Оплата визита') as Description -- Добавляем заметку из визита
			CASE 
				WHEN t.VisitId IS NOT NULL THEN 'Оплата визита'
				WHEN s.Id IS NOT NULL THEN 'Продажа: ' || tea.Name || ' (' || s.Grams || 'г)'
				ELSE 'Пополнение баланса / Прочее'
			END as Description
		FROM Transactions t
		LEFT JOIN Users u ON t.UserId = u.Id
		LEFT JOIN Sales s ON t.Amount = s.TotalCost AND t.CreatedAt = s.CreatedAt -- Связка по сумме и времени
		LEFT JOIN Teas tea ON s.TeaId = tea.Id
		WHERE t.CreatedAt BETWEEN @From AND @To
		ORDER BY t.CreatedAt DESC;";

        return await connection.QueryAsync(sql, new { From = from, To = to });
    }
}