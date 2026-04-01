using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.DataAccess.Interfaces;

/// <summary>
///     Интерфейс для управления данными о чае и складских остатках.
/// </summary>
public interface ITeaRepository
{
    /// <summary>
    ///     Получает полную информацию о сорте чая по его идентификатору.
    /// </summary>
    Task<Tea?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Получает актуальную цену (последнюю версию) для указанного чая.
    /// </summary>
    /// <param name="teaId">ID чая.</param>
    /// <returns>Объект цены со снимком стоимости для заварки и продажи.</returns>
    Task<TeaPrice?> GetLatestPriceAsync(Guid teaId);

    /// <summary>
    ///     Обновляет количество чая на складе.
    /// </summary>
    /// <param name="teaId">ID чая.</param>
    /// <param name="amount">Количество (отрицательное для списания, положительное для прихода).</param>
    /// <param name="transaction">Активная SQL-транзакция (Dapper).</param>
    /// <returns>True, если обновление прошло успешно.</returns>
    Task<bool> UpdateStockAsync(Guid teaId, decimal amount, IDbTransaction transaction);
}
