using DrinkTea.BL.Interfaces;
using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Shared.Enums;
using DrinkTea.Domain.Entities;

namespace DrinkTea.BL.Services;

public class VisitService(IUnitOfWork unitOfWork, IVisitRepository visitRepo) : IVisitService
{
    public async Task<Visit> StartVisitAsync(Guid? userId, string? note)
    {
        if (userId == Guid.Empty) userId = null;

        // КРИТИЧЕСКАЯ ПРОВЕРКА: Если это не постоянщик, то заметка (номер стола/имя) обязательна
        if (!userId.HasValue && string.IsNullOrWhiteSpace(note))
        {
            throw new ArgumentException("Для анонимного гостя необходимо указать имя или номер стола.");
        }

        if (userId.HasValue)
        {
            var alreadyInClub = await visitRepo.HasActiveVisitAsync(userId.Value);
            if (alreadyInClub) throw new InvalidOperationException("Пользователь уже в клубе.");
        }

        return await visitRepo.CreateAsync(userId, note);
    }


    /// <summary>
    /// 	Закрывает визит. Оплата проводится ровно на указанные суммы.
    /// </summary>
    public async Task CheckoutAsync(Guid visitId, decimal internalAmount, decimal externalAmount, PaymentMethod method, Guid staffId)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var visit = await visitRepo.GetByIdAsync(visitId)
                ?? throw new Exception("Визит не найден.");

            if (visit.IsClosed) throw new Exception("Визит уже закрыт.");

            // Проверка: сумма оплат должна быть не меньше долга
            if (internalAmount + externalAmount < visit.TotalAmount)
                throw new Exception($"Недостаточно средств. Долг: {visit.TotalAmount}");

            // 1. Списание с депозита (Internal)
            if (internalAmount > 0)
            {
                if (!visit.UserId.HasValue) throw new Exception("Аноним не может платить с депозита.");

                var success = await visitRepo.UpdateUserBalanceAsync(visit.UserId.Value, -internalAmount, transaction.DbTransaction);
                if (!success) throw new Exception("Ошибка списания с баланса.");

                await visitRepo.RegisterTransactionAsync(new Transaction
                {
                    VisitId = visitId,
                    UserId = visit.UserId,
                    StaffId = staffId,
                    Amount = internalAmount,
                    PaymentMethod = PaymentMethod.Internal,
                    Description = "Оплата визита (Депозит)"
                }, transaction.DbTransaction);
            }

            // 2. Внешняя оплата (Cash/Card)
            if (externalAmount > 0)
            {
                await visitRepo.RegisterTransactionAsync(new Transaction
                {
                    VisitId = visitId,
                    UserId = visit.UserId,
                    Amount = externalAmount,
                    PaymentMethod = method,
                    StaffId = staffId,
                    Description = $"Оплата визита ({method})"
                }, transaction.DbTransaction);
            }

            // 3. Закрытие визита (сумма зафиксирована, EndTime проставлен)
            await visitRepo.CloseAsync(visitId, transaction.DbTransaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task PayForFriendAsync(Guid payerUserId, Guid targetVisitId, Guid staffId)
    {
        using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var visit = await visitRepo.GetByIdAsync(targetVisitId) ?? throw new Exception("Визит не найден.");
            if (visit.IsClosed) throw new Exception("Визит уже оплачен.");

            // Списываем строго сумму долга
            var success = await visitRepo.UpdateUserBalanceAsync(payerUserId, -visit.TotalAmount, transaction.DbTransaction);
            if (!success) throw new Exception("Недостаточно средств на балансе плательщика.");

            await visitRepo.RegisterTransactionAsync(new Transaction
            {
                VisitId = targetVisitId,
                UserId = payerUserId,
                StaffId = staffId,
                Amount = visit.TotalAmount,
                PaymentMethod = PaymentMethod.Internal,
                Description = $"Оплата за друга (Визит: {visit.Note ?? "ID " + targetVisitId})"
            }, transaction.DbTransaction);

            await visitRepo.CloseAsync(targetVisitId, transaction.DbTransaction);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<dynamic>> GetActiveDashboardAsync() => await visitRepo.GetActiveVisitsWithNamesAsync();

    public async Task<IEnumerable<dynamic>> GetRawDailyReportAsync(DateTime date)
        => await GetRawReportAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));

    public async Task<IEnumerable<dynamic>> GetRawDetailedReportAsync(DateTime date)
        => await GetRawDetailedReportAsync(date.Date, date.Date.AddDays(1).AddTicks(-1));

    public async Task<IEnumerable<dynamic>> GetRawReportAsync(DateTime from, DateTime to)
        => await visitRepo.GetPaymentsSummaryAsync(from, to);

    public async Task<IEnumerable<dynamic>> GetRawDetailedReportAsync(DateTime from, DateTime to)
        => await visitRepo.GetDetailedTransactionsAsync(from, to);
}
