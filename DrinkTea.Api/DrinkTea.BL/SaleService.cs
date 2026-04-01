using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Shared.Enums;
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
    /// <param name="staffId">	ID мастера, совершившего продажу (из JWT). </param>
    public async Task<Guid> SellAsync(Guid teaId, decimal grams, PaymentMethod method, Guid staffId, Guid? userId = null)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Достаем данные о чае (чтобы знать название для истории)
            var tea = await teaRepo.GetByIdAsync(teaId)
                ?? throw new Exception("Чай не найден.");

            var price = await teaRepo.GetLatestPriceAsync(teaId, transaction)
                ?? throw new Exception("Цена не найдена.");

            decimal totalCost = grams * price.SalePricePerGram;

            // 2. Списываем склад
            var stockUpdated = await teaRepo.UpdateStockAsync(teaId, -grams, transaction);
            if (!stockUpdated) throw new Exception("Недостаточно чая на складе.");

            // 3. Если оплата с баланса — списываем
            if (method == PaymentMethod.Internal)
            {
                if (!userId.HasValue) throw new Exception("Для оплаты с депозита нужен клиент.");
                await visitRepo.UpdateUserBalanceAsync(userId.Value, -totalCost, transaction);
            }

            // 4. Регистрируем продажу
            await saleRepo.CreateSaleAsync(teaId, userId, grams, totalCost, method, staffId, transaction);

            // 5. Создаем подробную финансовую транзакцию
            await visitRepo.RegisterTransactionAsync(new Transaction
            {
                UserId = userId,
                StaffId = staffId, // Кто продал
                Amount = totalCost,
                PaymentMethod = method,
                // Используем имя чая прямо здесь:
                Description = $"Розничная продажа: {tea.Name} ({grams}г)",
                CreatedAt = DateTime.UtcNow
            }, transaction);

            transaction.Commit();
            return Guid.NewGuid();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

}
