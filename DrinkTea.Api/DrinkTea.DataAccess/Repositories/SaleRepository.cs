using Dapper;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Shared.Enums;
using System.Data;

namespace DrinkTea.DataAccess.Repositories;

/// <summary>
/// 	Реализация репозитория розничных продаж через Dapper.
/// </summary>
public class SaleRepository(DbConnectionFactory db) : ISaleRepository
{
    public async Task CreateSaleAsync(Guid teaId, Guid? userId, decimal grams, decimal totalCost, PaymentMethod method, Guid staffId, IDbTransaction transaction)
    {
        const string sql = @"
	        INSERT INTO Sales (TeaId, UserId, Grams, TotalCost, PaymentMethod, StaffId)
	        VALUES (@TeaId, @UserId, @Grams, @TotalCost, @Method, @StaffId);";

        await transaction.Connection.ExecuteAsync(sql,
            new { TeaId = teaId, UserId = userId, Grams = grams, TotalCost = totalCost, Method = method, StaffId = staffId },
            transaction);
    }
}