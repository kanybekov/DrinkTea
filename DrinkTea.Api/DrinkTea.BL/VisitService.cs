using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using System.Data;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис для управления визитами гостей и их финансовыми операциями.
/// </summary>
public class VisitService(
    DbConnectionFactory db,
    IVisitRepository visitRepo)
{
    /// <summary>
    /// 	Открывает новый визит, предотвращая дубликаты для постоянных клиентов.
    /// </summary>
    public async Task<Visit> StartVisitAsync(Guid? userId)
    {
        if (userId.HasValue)
        {
            // Бизнес-правило: один постоянщик — один активный визит
            var alreadyInClub = await visitRepo.HasActiveVisitAsync(userId.Value);
            if (alreadyInClub)
            {
                throw new InvalidOperationException("У этого пользователя уже есть открытый визит. Сначала закройте текущий счет.");
            }
        }

        return await visitRepo.CreateAsync(userId);
    }

    /// <summary>
    /// 	Проводит оплату и закрывает визит.
    /// </summary>
    /// <remarks>
    /// 	Поддерживает гибридную оплату (часть с депозита, часть внешним методом).
    /// </remarks>
    public async Task CheckoutAsync(Guid visitId, decimal internalAmount, decimal externalAmount, string method)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var visit = await visitRepo.GetByIdAsync(visitId)
                ?? throw new Exception("Визит не найден.");

            if (visit.IsClosed)
                throw new Exception("Визит уже был закрыт ранее.");

            // Проверка: сумма оплат должна покрывать накопленный долг (или быть больше)
            if (internalAmount + externalAmount < visit.TotalAmount)
                throw new Exception("Суммы недостаточно для закрытия счета.");

            // 1. Списание с депозита (Internal)
            if (internalAmount > 0)
            {
                if (visit.UserId == null)
                    throw new Exception("Анонимный гость не может платить с депозита.");

                var success = await visitRepo.UpdateUserBalanceAsync(visit.UserId.Value, -internalAmount, transaction);
                if (!success) throw new Exception("Ошибка обновления баланса пользователя.");

                await visitRepo.RegisterTransactionAsync(new Transaction
                {
                    VisitId = visitId,
                    UserId = visit.UserId,
                    Amount = internalAmount,
                    PaymentMethod = "Internal"
                }, transaction);
            }

            // 2. Внешняя оплата (Cash/Card)
            if (externalAmount > 0)
            {
                await visitRepo.RegisterTransactionAsync(new Transaction
                {
                    VisitId = visitId,
                    UserId = visit.UserId,
                    Amount = externalAmount,
                    PaymentMethod = method
                }, transaction);
            }

            // 3. Закрытие визита
            await visitRepo.CloseAsync(visitId, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 	Возвращает список всех гостей, которые сейчас находятся в клубе.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetActiveDashboardAsync()
    {
        return await visitRepo.GetActiveVisitsWithNamesAsync();
    }

    /// <summary>
    /// 	Получает полную информацию о визите по его идентификатору.
    /// </summary>
    /// <param name="id">	Уникальный идентификатор визита. </param>
    /// <returns>	Объект визита (Domain Entity). </returns>
    public async Task<Visit?> GetVisitByIdAsync(Guid id)
    {
        return await visitRepo.GetByIdAsync(id);
    }
}