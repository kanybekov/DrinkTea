using DrinkTea.DataAccess;
using DrinkTea.DataAccess.Interfaces;
using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Models.Requests;
using DrinkTea.Shared.Models.Responses;

namespace DrinkTea.BL.Services;

public class BrewingService(
    DbConnectionFactory db,
    ITeaRepository teaRepo,
    IBrewingRepository brewingRepo,
    IVisitRepository visitRepo)
{
    /// <summary>
    /// 	Создать новую заварку. Сумма делится на всех, остаток копеек падает на первого.
    /// </summary>
    public async Task<Guid> StartBrewingAsync(Guid teaId, decimal grams, List<Guid> visitIds, Guid userId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            if (visitIds == null || visitIds.Count == 0) throw new Exception("Нужен хотя бы один участник");

            var price = await teaRepo.GetLatestPriceAsync(teaId, transaction)
                ?? throw new Exception("Цена на выбранный чай не установлена");

            var stockUpdated = await teaRepo.UpdateStockAsync(teaId, -grams, transaction);
            if (!stockUpdated) throw new Exception("Недостаточно чая на складе");

            decimal totalCost = grams * price.BrewPricePerGram;

            // Расчет долей с округлением
            decimal baseShare = Math.Round(totalCost / visitIds.Count, 2, MidpointRounding.AwayFromZero);
            decimal remainder = totalCost - (baseShare * visitIds.Count);

            var sessionId = await brewingRepo.CreateSessionAsync(teaId, price.Id, grams, totalCost, userId, transaction);

            for (int i = 0; i < visitIds.Count; i++)
            {
                // Добавляем остаток копеек первому участнику
                decimal finalShare = (i == 0) ? baseShare + remainder : baseShare;

                await brewingRepo.AddParticipantAsync(sessionId, visitIds[i], finalShare, transaction);
                await visitRepo.AddToTotalAsync(visitIds[i], finalShare, transaction);
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
    /// 	Подсадка гостя. Полный пересчет всех долей, чтобы сумма всегда была равна TotalCost.
    /// </summary>
    public async Task JoinSessionAsync(Guid sessionId, Guid visitId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var session = await brewingRepo.GetSessionByIdAsync(sessionId, transaction);
            var participants = await brewingRepo.GetParticipantsBySessionIdAsync(sessionId, transaction);

            if (participants.Any(p => p.VisitId == visitId))
                throw new Exception("Этот гость уже участвует.");

            // 1. Сначала возвращаем всем старые доли (обнуляем влияние этой сессии на счета)
            foreach (var p in participants)
            {
                await visitRepo.AddToTotalAsync(p.VisitId, -p.ShareCost, transaction);
            }

            // 2. Считаем новые доли на (N+1) человек
            int newCount = participants.Count + 1;
            decimal newBaseShare = Math.Round(session.TotalCost / newCount, 2, MidpointRounding.AwayFromZero);
            decimal newRemainder = session.TotalCost - (newBaseShare * newCount);

            // 3. Обновляем старых участников
            for (int i = 0; i < participants.Count; i++)
            {
                decimal updatedShare = (i == 0) ? newBaseShare + newRemainder : newBaseShare;

                // Обновляем долю в таблице сессии
                await brewingRepo.UpdateParticipantShareAsync(sessionId, participants[i].VisitId, updatedShare, transaction);
                // Начисляем новую долю в визит
                await visitRepo.AddToTotalAsync(participants[i].VisitId, updatedShare, transaction);
            }

            // 4. Добавляем нового гостя
            await brewingRepo.AddParticipantAsync(sessionId, visitId, newBaseShare, transaction);
            await visitRepo.AddToTotalAsync(visitId, newBaseShare, transaction);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 	Выход гостя. Пересчет суммы на оставшихся.
    /// </summary>
    public async Task LeaveSessionAsync(Guid sessionId, Guid visitId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var session = await brewingRepo.GetSessionByIdAsync(sessionId, transaction);
            var participants = await brewingRepo.GetParticipantsBySessionIdAsync(sessionId, transaction);

            if (participants.Count <= 1) throw new Exception("Нельзя удалить последнего. Отмените заварку.");

            var toRemove = participants.FirstOrDefault(p => p.VisitId == visitId)
                ?? throw new Exception("Участник не найден.");

            // 1. Возвращаем деньги уходящему и удаляем его
            await visitRepo.AddToTotalAsync(visitId, -toRemove.ShareCost, transaction);
            await brewingRepo.RemoveParticipantAsync(sessionId, visitId, transaction);

            // 2. Пересчитываем доли для оставшихся
            var remaining = participants.Where(p => p.VisitId != visitId).ToList();
            decimal newBaseShare = Math.Round(session.TotalCost / remaining.Count, 2, MidpointRounding.AwayFromZero);
            decimal newRemainder = session.TotalCost - (newBaseShare * remaining.Count);

            for (int i = 0; i < remaining.Count; i++)
            {
                // Снимаем старую долю
                await visitRepo.AddToTotalAsync(remaining[i].VisitId, -remaining[i].ShareCost, transaction);

                // Считаем и начисляем новую
                decimal updatedShare = (i == 0) ? newBaseShare + newRemainder : newBaseShare;
                await brewingRepo.UpdateParticipantShareAsync(sessionId, remaining[i].VisitId, updatedShare, transaction);
                await visitRepo.AddToTotalAsync(remaining[i].VisitId, updatedShare, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task CancelSessionAsync(Guid sessionId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var session = await brewingRepo.GetSessionByIdAsync(sessionId, transaction);
            var participants = await brewingRepo.GetParticipantsBySessionIdAsync(sessionId, transaction);

            await teaRepo.UpdateStockAsync(session.TeaId, session.TotalGrams, transaction);

            foreach (var p in participants)
            {
                await visitRepo.AddToTotalAsync(p.VisitId, -p.ShareCost, transaction);
            }

            await brewingRepo.DeleteSessionAsync(sessionId, transaction);
            transaction.Commit();
        }
        catch { transaction.Rollback(); throw; }
    }

    public async Task FinishSessionAsync(Guid sessionId)
    {
        using var connection = db.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            await brewingRepo.FinishSessionAsync(sessionId, transaction);
            transaction.Commit();
        }
        catch { transaction.Rollback(); throw; }
    }

    public async Task<IEnumerable<dynamic>> GetVisitHistoryAsync(Guid visitId) => await visitRepo.GetVisitItemsAsync(visitId);

    public async Task<IEnumerable<ActiveBrewingDto>> GetActiveSessionsAsync()
    {
        var rawData = await brewingRepo.GetActiveSessionsWithParticipantsAsync();

        return rawData.Select(x => {
            // Приводим к словарю с игнорированием регистра ключей
            var row = new Dictionary<string, object>((IDictionary<string, object>)x, StringComparer.OrdinalIgnoreCase);

            // Безопасно достаем JSON участников
            string json = row.ContainsKey("participantsjson")
                ? row["participantsjson"]?.ToString() ?? "[]"
                : "[]";

            return new ActiveBrewingDto
            {
                Id = row.ContainsKey("id") ? (Guid)row["id"] : Guid.Empty,
                TeaName = row.ContainsKey("teaname") ? row["teaname"]?.ToString() ?? "Без названия" : "Без названия",
                Grams = row.ContainsKey("grams") ? Convert.ToDecimal(row["grams"]) : 0m,
                TotalCost = row.ContainsKey("totalcost") ? Convert.ToDecimal(row["totalcost"]) : 0m,
                CreatedAt = row.ContainsKey("createdat") ? (DateTime)row["createdat"] : DateTime.Now,

                // Десериализация с защитой от null
                Participants = System.Text.Json.JsonSerializer.Deserialize<List<ParticipantDto>>(json)
                               ?? new List<ParticipantDto>()
            };
        }).ToList();
    }


}
