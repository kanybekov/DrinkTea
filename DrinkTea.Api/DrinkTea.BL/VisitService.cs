using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Shared.Enums;
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
    public async Task<Visit> StartVisitAsync(Guid? userId, string? note)
    {
        if (userId == Guid.Empty) userId = null;

        if (userId.HasValue)
        {
            var alreadyInClub = await visitRepo.HasActiveVisitAsync(userId.Value);
            if (alreadyInClub) throw new InvalidOperationException("Пользователь уже в клубе.");
        }

        return await visitRepo.CreateAsync(userId, note);
    }

    /// <summary>
    /// 	Проводит оплату и закрывает визит.
    /// </summary>
    /// <remarks>
    /// 	Поддерживает гибридную оплату (часть с депозита, часть внешним методом).
    /// </remarks>
    public async Task CheckoutAsync(Guid visitId, decimal internalAmount, decimal externalAmount, PaymentMethod method, Guid staffId)
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
                    StaffId = staffId,
                    Amount = internalAmount, // БЫЛО: externalAmount
                    PaymentMethod = PaymentMethod.Internal, // Явно указываем Internal
                    Description = "Оплата визита (Депозит)"
                }, transaction);
            }

            // 2. Внешняя оплата (Cash/Card)
            if (externalAmount > 0)
            {
                await visitRepo.RegisterTransactionAsync(new Transaction
                {
                    VisitId = visitId,
                    UserId = visit.UserId, // Для анонима тут будет null, это правильно
                    Amount = externalAmount,
                    PaymentMethod = method,
                    StaffId = staffId,
                    Description = "Оплата визита"
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

    /// <summary>
    /// 	Получает агрегированные данные по платежам за день.
    /// </summary>
    /// <param name="date">	Выбранная дата. </param>
    /// <returns>	Список анонимных объектов из базы. </returns>
    public async Task<IEnumerable<dynamic>> GetRawDailyReportAsync(DateTime date)
    {
        var from = date.Date;
        var to = from.AddDays(1).AddTicks(-1);

        return await visitRepo.GetPaymentsSummaryAsync(from, to);
    }

    /// <summary>
    /// 	Получает детальный лог всех денежных операций за день.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetRawDetailedReportAsync(DateTime date)
    {
        var from = date.Date;
        var to = from.AddDays(1).AddTicks(-1);

        return await visitRepo.GetDetailedTransactionsAsync(from, to);
    }

    /// <summary>
    /// 	Оплата визита (счета) одного гостя с депозита другого пользователя.
    /// </summary>
    /// <param name="payerUserId">	ID того, кто платит (постоянщик). </param>
    /// <param name="targetVisitId"> ID визита, который нужно закрыть (друг/аноним). </param>
    public async Task PayForFriendAsync(Guid payerUserId, Guid targetVisitId, Guid staffId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Получаем визит друга
            var visit = await visitRepo.GetByIdAsync(targetVisitId)
                ?? throw new Exception("Визит друга не найден.");

            if (visit.IsClosed) throw new Exception("Визит уже оплачен.");

            // 2. Списываем всю сумму визита с баланса плательщика
            var success = await visitRepo.UpdateUserBalanceAsync(payerUserId, -visit.TotalAmount, transaction);
            if (!success) throw new Exception("Недостаточно средств на балансе плательщика.");

            // 3. Фиксируем транзакцию (кто платил и за какой визит)
            // В методе PayForFriendAsync
            await visitRepo.RegisterTransactionAsync(new Transaction
            {
                VisitId = targetVisitId,
                UserId = payerUserId,
                StaffId = staffId, // Нужно добавить в параметры метода
                Amount = visit.TotalAmount,
                PaymentMethod = PaymentMethod.Internal,
                Description = $"Оплата за друга (Визит: {visit.Note ?? "без метки"})"
            }, transaction);


            // 4. Закрываем визит друга
            await visitRepo.CloseAsync(targetVisitId, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}