using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using System.Data;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис управления процессом чаепития.
/// </summary>
public class BrewingService(
    DbConnectionFactory db,
    ITeaRepository teaRepo,
    IBrewingRepository brewingRepo,
    IVisitRepository visitRepo)
{
    /// <summary>
    /// 	Запускает новую сессию заваривания для группы гостей.
    /// </summary>
    /// <param name="teaId">	Какой чай завариваем. </param>
    /// <param name="grams">	Сколько грамм кладем в гайвань/чайник. </param>
    /// <param name="visitIds">	Список открытых визитов участников. </param>
    public async Task<Guid> StartBrewingAsync(Guid teaId, decimal grams, List<Guid> visitIds)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Получаем цену
            var price = await teaRepo.GetLatestPriceAsync(teaId)
                ?? throw new Exception("Цена на выбранный чай не установлена");

            // 2. Списываем чай
            var stockUpdated = await teaRepo.UpdateStockAsync(teaId, -grams, transaction);
            if (!stockUpdated) throw new Exception("Недостаточно чая на складе");

            // 3. Считаем деньги
            decimal totalCost = grams * price.BrewPricePerGram;
            decimal share = totalCost / visitIds.Count;

            // 4. Сохраняем сессию
            var sessionId = await brewingRepo.CreateSessionAsync(teaId, price.Id, grams, totalCost, transaction);

            // 5. Разносим доли по визитам
            foreach (var vId in visitIds)
            {
                await brewingRepo.AddParticipantAsync(sessionId, vId, share, transaction);
                await visitRepo.AddToTotalAsync(vId, share, transaction);
            }

            transaction.Commit();
            return sessionId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 	Добавляет гостя в уже активную сессию заваривания с пересчетом долей.
    /// </summary>
    /// <remarks>
    /// 	Сумма сессии делится на (N+1) участников. У старых участников 
    /// 	излишек вычитается из счета визита, новому — начисляется актуальная доля.
    /// </remarks>
    public async Task JoinSessionAsync(Guid sessionId, Guid visitId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Извлекаем данные для расчета
            var session = await brewingRepo.GetSessionByIdAsync(sessionId, transaction);
            var currentParticipants = await brewingRepo.GetParticipantsBySessionIdAsync(sessionId, transaction);

            if (currentParticipants.Any(p => p.VisitId == visitId))
                throw new Exception("Этот гость уже пьет этот чай.");

            // 2. Математика пересчета
            int oldPeopleCount = currentParticipants.Count;
            int newPeopleCount = oldPeopleCount + 1;

            decimal oldShare = session.TotalCost / oldPeopleCount;
            decimal newShare = session.TotalCost / newPeopleCount;
            decimal refundAmount = oldShare - newShare; // Сколько "вернуть" старым гостям

            // 3. Уменьшаем долг старых участников
            foreach (var participant in currentParticipants)
            {
                // Передаем отрицательное число, чтобы UPDATE сработал на вычитание
                await visitRepo.AddToTotalAsync(participant.VisitId, -refundAmount, transaction);
            }

            // 4. Обновляем записи долей в таблице сессии
            await brewingRepo.UpdateAllSharesInSessionAsync(sessionId, newShare, transaction);

            // 5. Добавляем нового счастливчика
            await brewingRepo.AddParticipantAsync(sessionId, visitId, newShare, transaction);
            await visitRepo.AddToTotalAsync(visitId, newShare, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 	Удаляет гостя из активной сессии заваривания (отмена участия).
    /// </summary>
    /// <remarks>
    /// 	Сумма визита гостя уменьшается на его долю, а стоимость для 
    /// 	оставшихся участников увеличивается (TotalCost делится на N-1).
    /// </remarks>
    public async Task LeaveSessionAsync(Guid sessionId, Guid visitId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var session = await brewingRepo.GetSessionByIdAsync(sessionId, transaction);
            var currentParticipants = await brewingRepo.GetParticipantsBySessionIdAsync(sessionId, transaction);

            var participantToRemove = currentParticipants.FirstOrDefault(p => p.VisitId == visitId)
                ?? throw new Exception("Этот гость не участвует в данной заварке.");

            if (currentParticipants.Count <= 1)
                throw new Exception("Нельзя удалить последнего участника. Отмените заварку целиком.");

            // 1. Возвращаем деньги на счет уходящего гостя
            await visitRepo.AddToTotalAsync(visitId, -participantToRemove.ShareCost, transaction);

            // 2. Удаляем запись из таблицы участников
            await brewingRepo.RemoveParticipantAsync(sessionId, visitId, transaction);

            // 3. Пересчитываем доли для тех, кто остался
            int remainingCount = currentParticipants.Count - 1;
            decimal newShare = session.TotalCost / remainingCount;
            decimal supplement = newShare - participantToRemove.ShareCost; // Сколько нужно "доплатить" остальным

            foreach (var p in currentParticipants.Where(p => p.VisitId != visitId))
            {
                await visitRepo.AddToTotalAsync(p.VisitId, supplement, transaction);
            }

            // 4. Обновляем доли в таблице сессии
            await brewingRepo.UpdateAllSharesInSessionAsync(sessionId, newShare, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 	Полная отмена сессии заваривания.
    /// </summary>
    /// <remarks>
    /// 	Возвращает списанный чай на склад и аннулирует доли в счетах всех участников.
    /// </remarks>
    public async Task CancelSessionAsync(Guid sessionId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Получаем данные сессии и список участников
            var session = await brewingRepo.GetSessionByIdAsync(sessionId, transaction);
            var participants = await brewingRepo.GetParticipantsBySessionIdAsync(sessionId, transaction);

            // 2. Возвращаем чай на склад (передаем положительное число для прихода)
            var teaRestored = await teaRepo.UpdateStockAsync(session.TeaId, session.TotalGrams, transaction);
            if (!teaRestored) throw new Exception("Ошибка при возврате чая на склад.");

            // 3. Вычитаем доли из счетов всех участников
            foreach (var p in participants)
            {
                await visitRepo.AddToTotalAsync(p.VisitId, -p.ShareCost, transaction);
            }

            // 4. Удаляем сессию (связанные участники удалятся автоматически по ON DELETE CASCADE в БД)
            await brewingRepo.DeleteSessionAsync(sessionId, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 	Получает историю всех заварок в рамках конкретного визита.
    /// </summary>
    /// <param name="visitId">	Уникальный идентификатор визита. </param>
    /// <returns>	Список анонимных объектов (данные из репозитория). </returns>
    public async Task<IEnumerable<dynamic>> GetVisitHistoryAsync(Guid visitId)
    {
        // Обращаемся к репозиторию визитов, который уже внедрен в конструктор сервиса
        return await visitRepo.GetVisitItemsAsync(visitId);
    }
}
