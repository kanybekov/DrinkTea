using Dapper;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.DataAccess.Repositories;

/// <summary>
/// 	Реализация репозитория для работы с чаем и ценами через Dapper.
/// </summary>
public class TeaRepository(DbConnectionFactory db) : ITeaRepository
{
    public async Task<Tea?> GetByIdAsync(Guid id)
    {
        using var connection = db.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Tea>(
            "SELECT * FROM Teas WHERE Id = @Id", new { Id = id });
    }

    public async Task<TeaPrice?> GetLatestPriceAsync(Guid teaId, IDbTransaction? transaction = null)
    {
        // Если транзакция передана — используем её соединение, иначе создаем новое
        var connection = transaction?.Connection ?? db.CreateConnection();

        const string sql = @"
		SELECT * FROM TeaPrices 
		WHERE TeaId = @TeaId 
		ORDER BY CreatedAt DESC LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<TeaPrice>(sql, new { TeaId = teaId }, transaction);
    }


    public async Task<bool> UpdateStockAsync(Guid teaId, decimal amount, IDbTransaction transaction)
    {
        // amount может быть отрицательным (списание)
        var sql = @"UPDATE Teas SET CurrentStock = CurrentStock + @Amount 
					WHERE Id = @TeaId AND (CurrentStock + @Amount) >= 0";

        var rows = await transaction.Connection.ExecuteAsync(sql,
            new { TeaId = teaId, Amount = amount }, transaction);

        return rows > 0;
    }

    public async Task<IEnumerable<dynamic>> GetInventoryAsync()
    {
        using var connection = db.CreateConnection();

        // Используем оконную функцию DISTINCT ON, чтобы взять только САМУЮ СВЕЖУЮ цену для каждого чая
        const string sql = @"
		SELECT 
			t.Id, 
			t.Name, 
			t.CurrentStock,
			p.BrewPricePerGram as BrewPrice,
			p.SalePricePerGram as SalePrice
		FROM Teas t
		LEFT JOIN LATERAL (
			SELECT BrewPricePerGram, SalePricePerGram
			FROM TeaPrices
			WHERE TeaId = t.Id
			ORDER BY CreatedAt DESC
			LIMIT 1
		) p ON TRUE
		ORDER BY t.Name;";

        return await connection.QueryAsync(sql);
    }

    public async Task CreateAsync(Tea tea, IDbTransaction transaction)
    {
        const string sql = "INSERT INTO Teas (Id, Name, CurrentStock) VALUES (@Id, @Name, @CurrentStock);";
        await transaction.Connection.ExecuteAsync(sql, tea, transaction);
    }

    public async Task AddPriceAsync(TeaPrice price, IDbTransaction transaction)
    {
        const string sql = @"
		INSERT INTO TeaPrices (TeaId, BrewPricePerGram, SalePricePerGram) 
		VALUES (@TeaId, @BrewPricePerGram, @SalePricePerGram);";
        await transaction.Connection.ExecuteAsync(sql, price, transaction);
    }

    public async Task UpsertPublicReviewAsync(Guid teaId, Guid userId, int rating, string comment, IDbTransaction transaction)
    {
        const string sql = @"
        INSERT INTO TeaPublicReviews (Id, TeaId, UserId, Rating, Comment, CreatedAt)
        VALUES (gen_random_uuid(), @TeaId, @UserId, @Rating, @Comment, CURRENT_TIMESTAMP)
        ON CONFLICT (TeaId, UserId)
        DO UPDATE SET
            Rating = EXCLUDED.Rating,
            Comment = EXCLUDED.Comment,
            CreatedAt = CURRENT_TIMESTAMP;";

        await transaction.Connection.ExecuteAsync(sql, new
        {
            TeaId = teaId,
            UserId = userId,
            Rating = rating,
            Comment = comment
        }, transaction);
    }

    public async Task UpsertPrivateNoteAsync(Guid teaId, Guid userId, string noteText, IDbTransaction transaction)
    {
        const string sql = @"
        INSERT INTO TeaPrivateNotes (Id, TeaId, UserId, NoteText, CreatedAt)
        VALUES (gen_random_uuid(), @TeaId, @UserId, @NoteText, CURRENT_TIMESTAMP)
        ON CONFLICT (TeaId, UserId)
        DO UPDATE SET
            NoteText = EXCLUDED.NoteText,
            CreatedAt = CURRENT_TIMESTAMP;";

        await transaction.Connection.ExecuteAsync(sql, new
        {
            TeaId = teaId,
            UserId = userId,
            NoteText = noteText
        }, transaction);
    }

    public async Task DeletePrivateNoteAsync(Guid teaId, Guid userId, IDbTransaction transaction)
    {
        const string sql = "DELETE FROM TeaPrivateNotes WHERE TeaId = @TeaId AND UserId = @UserId;";
        await transaction.Connection.ExecuteAsync(sql, new { TeaId = teaId, UserId = userId }, transaction);
    }

    public async Task<bool> DeletePublicReviewByOwnerAsync(Guid teaId, Guid userId, IDbTransaction transaction)
    {
        const string sql = "DELETE FROM TeaPublicReviews WHERE TeaId = @TeaId AND UserId = @UserId;";
        var affected = await transaction.Connection.ExecuteAsync(sql, new { TeaId = teaId, UserId = userId }, transaction);
        return affected > 0;
    }

    public async Task<bool> DeletePublicReviewByIdAsync(Guid teaId, Guid reviewId, IDbTransaction transaction)
    {
        const string sql = "DELETE FROM TeaPublicReviews WHERE TeaId = @TeaId AND Id = @ReviewId;";
        var affected = await transaction.Connection.ExecuteAsync(sql, new { TeaId = teaId, ReviewId = reviewId }, transaction);
        return affected > 0;
    }

    public async Task<Tea?> GetTeaWithFeedbackAsync(Guid teaId, Guid? currentUserId)
    {
        using var connection = db.CreateConnection();
        const string teaSql = "SELECT Id, Name, CurrentStock, Unit FROM Teas WHERE Id = @TeaId;";
        var tea = await connection.QueryFirstOrDefaultAsync<Tea>(teaSql, new { TeaId = teaId });
        if (tea == null)
        {
            return null;
        }

        const string publicSql = @"
        SELECT
            r.Id,
            r.TeaId,
            r.Rating,
            r.Comment,
            r.UserId,
            COALESCE(u.FullName, '') AS UserName,
            r.CreatedAt
        FROM TeaPublicReviews r
        LEFT JOIN Users u ON u.Id = r.UserId
        WHERE r.TeaId = @TeaId
        ORDER BY r.CreatedAt DESC;";

        var publicReviews = await connection.QueryAsync<PublicReview>(publicSql, new { TeaId = teaId });
        tea.PublicReviews = publicReviews.ToList();

        if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
        {
            const string privateSql = @"
            SELECT Id, TeaId, UserId, NoteText, CreatedAt
            FROM TeaPrivateNotes
            WHERE TeaId = @TeaId AND UserId = @UserId
            ORDER BY CreatedAt DESC;";

            var privateNotes = await connection.QueryAsync<PrivateNote>(privateSql, new
            {
                TeaId = teaId,
                UserId = currentUserId.Value
            });
            tea.PrivateNotes = privateNotes.ToList();
        }

        return tea;
    }

    public async Task<IEnumerable<Tea>> GetTeasForRatingsAsync(Guid? currentUserId)
    {
        using var connection = db.CreateConnection();
        var teas = (await connection.QueryAsync<Tea>("SELECT Id, Name, CurrentStock, Unit FROM Teas;")).ToList();
        if (!teas.Any())
        {
            return teas;
        }

        const string publicSql = @"
        SELECT
            r.Id,
            r.TeaId,
            r.Rating,
            r.Comment,
            r.UserId,
            COALESCE(u.FullName, '') AS UserName,
            r.CreatedAt
        FROM TeaPublicReviews r
        LEFT JOIN Users u ON u.Id = r.UserId
        ORDER BY r.CreatedAt DESC;";
        var allReviews = (await connection.QueryAsync<PublicReview>(publicSql)).ToList();

        Dictionary<Guid, List<PrivateNote>> privateByTea = new();
        if (currentUserId.HasValue && currentUserId.Value != Guid.Empty)
        {
            const string privateSql = @"
            SELECT Id, TeaId, UserId, NoteText, CreatedAt
            FROM TeaPrivateNotes
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC;";
            var allNotes = await connection.QueryAsync<PrivateNote>(privateSql, new { UserId = currentUserId.Value });
            privateByTea = allNotes.GroupBy(x => x.TeaId).ToDictionary(g => g.Key, g => g.ToList());
        }

        var reviewsByTea = allReviews.GroupBy(x => x.TeaId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var tea in teas)
        {
            if (reviewsByTea.TryGetValue(tea.Id, out var reviews))
            {
                tea.PublicReviews = reviews;
            }

            if (privateByTea.TryGetValue(tea.Id, out var notes))
            {
                tea.PrivateNotes = notes;
            }
        }

        return teas;
    }

    public async Task<IEnumerable<dynamic>> GetMyTeaRatingsAsync(Guid userId)
    {
        using var connection = db.CreateConnection();
        const string sql = @"
        SELECT
            t.id as TeaId,
            t.name as TeaName,
            pr.rating as MyRating,
            pr.comment as MyComment,
            pr.createdat as LastRatedAt,
            pn.notetext as MyPrivateNote,
            pn.createdat as LastNotedAt
        FROM teas t
        LEFT JOIN LATERAL (
            SELECT rating, comment, createdat
            FROM teapublicreviews
            WHERE teaid = t.id AND userid = @UserId
            ORDER BY createdat DESC
            LIMIT 1
        ) pr ON TRUE
        LEFT JOIN LATERAL (
            SELECT notetext, createdat
            FROM teaprivatenotes
            WHERE teaid = t.id AND userid = @UserId
            ORDER BY createdat DESC
            LIMIT 1
        ) pn ON TRUE
        WHERE pr.rating IS NOT NULL OR pn.notetext IS NOT NULL
        ORDER BY COALESCE(pr.createdat, pn.createdat) DESC, t.name;";

        return await connection.QueryAsync(sql, new { UserId = userId });
    }

    public async Task AddPriceSnapshotAsync(Guid teaId, decimal brewPrice, decimal salePrice, IDbTransaction transaction)
    {
        const string sql = @"
            INSERT INTO teaprices (id, teaid, brewpricepergram, salepricepergram, createdat)
            VALUES (gen_random_uuid(), @TeaId, @BrewPrice, @SalePrice, CURRENT_TIMESTAMP);";
        await transaction.Connection.ExecuteAsync(sql, new { TeaId = teaId, BrewPrice = brewPrice, SalePrice = salePrice }, transaction);
    }
}
