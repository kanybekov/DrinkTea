using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис для управления справочником чая и складскими остатками.
/// </summary>
public class TeaService(ITeaRepository teaRepo, DbConnectionFactory db)
{
    /// <summary>
    /// 	Получает актуальное состояние склада для всех позиций.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetFullInventoryAsync()
    {
        return await teaRepo.GetInventoryAsync();
    }

    /// <summary>
	/// 	Регистрирует новый чай и устанавливает для него начальные цены.
	/// </summary>
	public async Task<Guid> CreateTeaWithPriceAsync(string name, decimal stock, decimal brewPrice, decimal salePrice)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var teaId = Guid.NewGuid();
            await teaRepo.CreateAsync(new Tea { Id = teaId, Name = name, CurrentStock = stock }, transaction);

            await teaRepo.AddPriceAsync(new TeaPrice
            {
                TeaId = teaId,
                BrewPricePerGram = brewPrice,
                SalePricePerGram = salePrice
            }, transaction);

            transaction.Commit();
            return teaId;
        }
        catch
        {
            transaction.Rollback(); throw;
        }
    }

    /// <summary>
    /// 	Пополняет остатки чая и опционально фиксирует изменение цены.
    /// </summary>
    /// <remarks>
    /// 	Если новые цены не указаны, обновляется только количество на складе.
    /// </remarks>
    public async Task RestockAsync(Guid teaId, decimal amount, decimal? newBrewPrice, decimal? newSalePrice)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Увеличиваем склад (метод репозитория принимает транзакцию)
            await teaRepo.UpdateStockAsync(teaId, amount, transaction);

            // 2. Логика обновления цены: создаем запись в TeaPrices, только если передана хотя бы одна цена
            if (newBrewPrice.HasValue || newSalePrice.HasValue)
            {
                // Получаем текущие актуальные цены, чтобы подставить их, если изменена только одна из двух
                var currentPrice = await teaRepo.GetLatestPriceAsync(teaId);

                await teaRepo.AddPriceAsync(new TeaPrice
                {
                    TeaId = teaId,
                    BrewPricePerGram = newBrewPrice ?? currentPrice?.BrewPricePerGram ?? 0,
                    SalePricePerGram = newSalePrice ?? currentPrice?.SalePricePerGram ?? 0
                }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


}