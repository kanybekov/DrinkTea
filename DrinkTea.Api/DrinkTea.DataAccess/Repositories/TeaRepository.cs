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
}
