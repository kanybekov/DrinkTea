using DrinkTea.BL.Interfaces;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Models.Responses;

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

    public async Task SaveFeedbackAsync(Guid teaId, Guid userId, int rating, string? comment, string? privateNote)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            await teaRepo.UpsertPublicReviewAsync(teaId, userId, rating, comment?.Trim() ?? string.Empty, transaction.DbTransaction);

            if (privateNote is not null)
            {
                if (string.IsNullOrWhiteSpace(privateNote))
                {
                    await teaRepo.DeletePrivateNoteAsync(teaId, userId, transaction.DbTransaction);
                }
                else
                {
                    await teaRepo.UpsertPrivateNoteAsync(teaId, userId, privateNote.Trim(), transaction.DbTransaction);
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteMyReviewAsync(Guid teaId, Guid userId)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var deleted = await teaRepo.DeletePublicReviewByOwnerAsync(teaId, userId, transaction.DbTransaction);
            await transaction.CommitAsync();
            return deleted;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteReviewByIdAsync(Guid teaId, Guid reviewId)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var deleted = await teaRepo.DeletePublicReviewByIdAsync(teaId, reviewId, transaction.DbTransaction);
            await transaction.CommitAsync();
            return deleted;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Tea?> GetTeaWithFeedbackAsync(Guid teaId, Guid? currentUserId)
    {
        return await teaRepo.GetTeaWithFeedbackAsync(teaId, currentUserId);
    }

    public async Task<IEnumerable<Tea>> GetTeasForRatingsAsync(Guid? currentUserId)
    {
        return await teaRepo.GetTeasForRatingsAsync(currentUserId);
    }

    public async Task<IEnumerable<MyTeaRatingItemResponse>> GetMyTeaRatingsAsync(Guid userId)
    {
        var raw = await teaRepo.GetMyTeaRatingsAsync(userId);
        return raw.Select(x => new MyTeaRatingItemResponse
        {
            TeaId = (Guid)x.teaid,
            TeaName = (string)x.teaname,
            MyRating = x.myrating == null ? null : (int?)x.myrating,
            MyComment = x.mycomment == null ? null : (string)x.mycomment,
            MyPrivateNote = x.myprivatenote == null ? null : (string)x.myprivatenote,
            LastRatedAt = x.lastratedat == null ? null : (DateTime?)x.lastratedat,
            LastNotedAt = x.lastnotedat == null ? null : (DateTime?)x.lastnotedat
        });
    }

}