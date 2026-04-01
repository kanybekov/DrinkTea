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

    public async Task<TeaPrice?> GetLatestPriceAsync(Guid teaId)
    {
        using var connection = db.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TeaPrice>(
            @"SELECT * FROM TeaPrices 
			  WHERE TeaId = @TeaId 
			  ORDER BY CreatedAt DESC LIMIT 1",
            new { TeaId = teaId });
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
}
