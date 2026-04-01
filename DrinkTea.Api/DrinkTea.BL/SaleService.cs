using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Common;
using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис для управления розничными продажами в магазине (Retail).
/// </summary>
public class SaleService(
    DbConnectionFactory db,
    ITeaRepository teaRepo,
    ISaleRepository saleRepo,
    IVisitRepository visitRepo)
{
    /// <summary>
    /// 	Оформляет продажу чая на вынос.
    /// </summary>
    /// <remarks>
    /// 	Списывает остаток со склада по розничной цене. 
    /// 	Если указан UserId и метод Internal — списывает средства с депозита.
    /// </remarks>
    public async Task<Guid> SellAsync(Guid teaId, decimal grams, PaymentMethod method, Guid? userId = null)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Получаем розничную цену
            var price = await teaRepo.GetLatestPriceAsync(teaId)
                ?? throw new Exception("Цена на чай не найдена.");

            decimal totalCost = grams * price.SalePricePerGram;

            // 2. Списываем склад
            var stockUpdated = await teaRepo.UpdateStockAsync(teaId, -grams, transaction);
            if (!stockUpdated) throw new Exception("Недостаточно чая на складе.");

            // 3. Если оплата с депозита — списываем баланс
            if (method == PaymentMethod.Internal)
            {
                if (!userId.HasValue) throw new Exception("Для оплаты с депозита нужен ID пользователя.");

                var balanceUpdated = await visitRepo.UpdateUserBalanceAsync(userId.Value, -totalCost, transaction);
                if (!balanceUpdated) throw new Exception("Ошибка списания с баланса пользователя.");
            }

            // 4. Регистрируем продажу и общую финансовую транзакцию
            await saleRepo.CreateSaleAsync(teaId, userId, grams, totalCost, method, transaction);

            await visitRepo.RegisterTransactionAsync(new Transaction
            {
                UserId = userId,
                Amount = totalCost,
                PaymentMethod = method
            }, transaction);

            transaction.Commit();
            return Guid.NewGuid(); // Возвращаем ID операции (упрощенно)
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
