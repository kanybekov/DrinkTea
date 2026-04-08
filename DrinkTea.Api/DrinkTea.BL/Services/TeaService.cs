using DrinkTea.BL.Interfaces;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис для управления справочником чая и складскими остатками.
/// </summary>
public class TeaService(ITeaRepository teaRepo, IUnitOfWork unitOfWork) : ITeaService
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
        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var teaId = Guid.NewGuid();
            await teaRepo.CreateAsync(new Tea { Id = teaId, Name = name, CurrentStock = stock }, transaction.DbTransaction);

            await teaRepo.AddPriceAsync(new TeaPrice
            {
                TeaId = teaId,
                BrewPricePerGram = brewPrice,
                SalePricePerGram = salePrice
            }, transaction.DbTransaction);

            await transaction.CommitAsync();
            return teaId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
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
        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Увеличиваем склад (метод репозитория принимает транзакцию)
            await teaRepo.UpdateStockAsync(teaId, amount, transaction.DbTransaction);

            // 2. Логика обновления цены: создаем запись в TeaPrices, только если передана хотя бы одна цена
            if (newBrewPrice.HasValue || newSalePrice.HasValue)
            {
                // Получаем текущие актуальные цены, чтобы подставить их, если изменена только одна из двух
                var currentPrice = await teaRepo.GetLatestPriceAsync(teaId, transaction.DbTransaction);

                await teaRepo.AddPriceAsync(new TeaPrice
                {
                    TeaId = teaId,
                    BrewPricePerGram = newBrewPrice ?? currentPrice?.BrewPricePerGram ?? 0,
                    SalePricePerGram = newSalePrice ?? currentPrice?.SalePricePerGram ?? 0
                }, transaction.DbTransaction);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateTeaPricesAsync(Guid teaId, decimal brewPrice, decimal salePrice)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            await teaRepo.AddPriceSnapshotAsync(teaId, brewPrice, salePrice, transaction.DbTransaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


}