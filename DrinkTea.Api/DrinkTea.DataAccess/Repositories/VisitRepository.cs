using Dapper;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Models.Responses;
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
    public async Task<bool> UpdateUserBalanceAsync(Guid userId, decimal amount, IDbTransaction? transaction)
    {
        // Если транзакция есть — берем её соединение. Если нет — создаем новое.
        var connection = transaction?.Connection ?? db.CreateConnection();

        const string sql = "UPDATE Users SET Balance = Balance + @Amount WHERE Id = @UserId;";

        // Передаем объект транзакции в Dapper (если там null, Dapper поймет)
        var rows = await connection.ExecuteAsync(sql, new { UserId = userId, Amount = amount }, transaction);

        return rows > 0;
    }


    public async Task RegisterTransactionAsync(Transaction tx, IDbTransaction transaction)
    {
        // Если транзакция есть — берем её соединение. Если нет — создаем новое.
        var connection = transaction?.Connection ?? db.CreateConnection();

        // 1. ПРОВЕРКА: Если в репозиторий пришли нули - это авария
        if (tx.StaffId == Guid.Empty)
        {
            throw new Exception("Критическая ошибка: Попытка записать транзакцию без ID мастера (StaffId is Empty).");
        }

        const string sql = @"
		INSERT INTO Transactions (Id, VisitId, UserId, StaffId, Amount, PaymentMethod, Description)
		VALUES (@Id, @VisitId, @UserId, @StaffId, @Amount, @PaymentMethod, @Description);";

        // 2. Явно указываем параметры, чтобы Dapper не ошибся с именами свойств
        await connection.ExecuteAsync(sql, new
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
            v.id as Id, 
            v.userid as UserId, -- Обязательно вытягиваем ID юзера
            u.fullname as UserName, 
            v.note as Note, 
            v.totalamount as UnpaidDebt, 
            u.balance as UserDeposit, 
            v.starttime as StartTime
        FROM visits v
        LEFT JOIN users u ON v.userid = u.id
        WHERE v.isclosed = FALSE
        ORDER BY v.starttime DESC;";

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
            -- КТО: Приоритет FullName, если NULL (аноним) — берем Note из визита
            COALESCE(u.FullName, v.Note, 'Розничный клиент') as Customer, 
    
            t.Amount, 
            t.PaymentMethod as Method, 
    
            -- ЧТО: Используем системный Description из таблицы Transactions, 
            -- который вы заполняете в BL (например, ""Оплата визита"", ""Пополнение"")
            t.Description as Operation,
    
            -- ДОП: Кто из мастеров провел операцию
            staff.FullName as MasterName
        FROM Transactions t
        LEFT JOIN Users u ON t.UserId = u.Id
        LEFT JOIN Visits v ON t.VisitId = v.Id
        LEFT JOIN Users staff ON t.StaffId = staff.Id
        WHERE t.CreatedAt BETWEEN @From AND @To
        ORDER BY t.CreatedAt DESC;
";

        return await connection.QueryAsync(sql, new { From = from, To = to });
    }
    
    public async Task<CustomerFullProfileResponse?> GetCustomerStatsAsync(Guid userId)
    {
        using var connection = db.CreateConnection();

        // Пишем всё строчными буквами, как в твоей БД. 
        // Если в БД колонки id, fullname, то пишем их так.
        const string sql = @"
    SELECT 
        u.id as Id, 
        u.fullname as Name, 
        u.balance as Balance,
        (SELECT COUNT(*)::int FROM visits WHERE userid = u.id AND isclosed = TRUE) as VisitsCount,
        (
            SELECT t.name 
            FROM brewingparticipants p
            JOIN brewingsessions s ON p.sessionid = s.id
            JOIN teas t ON s.teaid = t.id
            JOIN visits v ON p.visitid = v.id
            WHERE v.userid = u.id
            GROUP BY t.name
            ORDER BY COUNT(*) DESC
            LIMIT 1
        ) as FavoriteTea
    FROM users u
    WHERE u.id = @UserId;";

        return await connection.QueryFirstOrDefaultAsync<CustomerFullProfileResponse>(sql, new { UserId = userId });
    }

    public async Task<List<LastBrewingDto>> GetUserVisitHistoryAsync(Guid userId, int limit = 5)
    {
        using var connection = db.CreateConnection();
        const string sql = @"
        SELECT 
            t.name as TeaName, 
            s.createdat as Date, 
            p.sharecost as Amount
        FROM brewingparticipants p
        JOIN brewingsessions s ON p.sessionid = s.id
        JOIN teas t ON s.teaid = t.id
        JOIN visits v ON p.visitid = v.id
        WHERE v.userid = @UserId AND v.isclosed = TRUE
        ORDER BY s.createdat DESC
        LIMIT @Limit;";

        var result = await connection.QueryAsync<LastBrewingDto>(sql, new { UserId = userId, Limit = limit });
        return result.ToList();
    }
}