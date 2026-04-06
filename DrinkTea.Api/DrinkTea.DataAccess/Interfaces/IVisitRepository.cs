using DrinkTea.Domain.Entities;
using DrinkTea.Shared.Models.Responses;
using System.Data;

namespace DrinkTea.DataAccess.Interfaces;

/// <summary>
/// 	Интерфейс для управления визитами (открытыми счетами) гостей клуба.
/// </summary>
public interface IVisitRepository
{
    /// <summary>
    /// 	Создает новую запись о визите при входе гостя (Check-in).
    /// </summary>
    /// <param name="userId">
    /// 	Идентификатор постоянного клиента. Если null — создается анонимный визит.
    /// </param>
    /// <returns>
    /// 	Созданный объект визита с присвоенным ID и временем начала.
    /// </returns>
    Task<Visit> CreateAsync(Guid? userId, string? note);

    /// <summary>
    /// 	Получает данные о визите по его идентификатору.
    /// </summary>
    /// <remarks>
    /// 	Используется для проверки статуса визита перед добавлением его в сессию заваривания.
    /// </remarks>
    Task<Visit?> GetByIdAsync(Guid id);

    /// <summary>
    /// 	Увеличивает накопленную сумму визита на стоимость выпитого чая.
    /// </summary>
    /// <param name="visitId">
    /// 	Идентификатор визита, которому начисляется сумма.
    /// </param>
    /// <param name="amount">
    /// 	Сумма доли в текущей сессии заваривания.
    /// </param>
    /// <param name="transaction">
    /// 	Активная SQL-транзакция для обеспечения целостности данных.
    /// </param>
    /// <returns>
    /// 	True, если баланс визита успешно обновлен.
    /// </returns>
    Task<bool> AddToTotalAsync(Guid visitId, decimal amount, IDbTransaction transaction);

    /// <summary>
    /// 	Закрывает визит (Checkout), фиксируя время окончания и способ оплаты.
    /// </summary>
    /// <remarks>
    /// 	После вызова этого метода добавление новых трат в визит запрещено.
    /// </remarks>
    Task<bool> CloseAsync(Guid visitId, IDbTransaction transaction);

    /// <summary>
    /// 	Списывает средства с личного баланса пользователя.
    /// </summary>
    Task<bool> UpdateUserBalanceAsync(Guid userId, decimal amount, IDbTransaction transaction);

    /// <summary>
    /// 	Регистрирует финансовую транзакцию в истории.
    /// </summary>
    Task RegisterTransactionAsync(Transaction tx, IDbTransaction transaction);

    /// <summary>
    /// 	Получает список всех незакрытых визитов с именами гостей.
    /// </summary>
    /// <returns> Список активных визитов для панели мастера. </returns>
    Task<IEnumerable<dynamic>> GetActiveVisitsWithNamesAsync();

    Task<IEnumerable<dynamic>> GetVisitItemsAsync(Guid visitId);

    /// <summary>
    /// 	Проверяет наличие незакрытого визита для конкретного пользователя.
    /// </summary>
    /// <param name="userId">	ID пользователя. </param>
    /// <returns>	True, если у пользователя уже есть активный счет. </returns>
    Task<bool> HasActiveVisitAsync(Guid userId);

    /// <summary>
    /// 	Получает агрегированную статистику по платежам за указанный период.
    /// </summary>
    Task<IEnumerable<dynamic>> GetPaymentsSummaryAsync(DateTime from, DateTime to);

    /// <summary>
    /// 	Получает полный список транзакций за период с именами пользователей.
    /// </summary>
    Task<IEnumerable<dynamic>> GetDetailedTransactionsAsync(DateTime from, DateTime to);

    Task<CustomerFullProfileResponse?> GetCustomerStatsAsync(Guid userId);

    Task<List<LastBrewingDto>> GetUserVisitHistoryAsync(Guid userId, int limit = 5);

    Task<List<LastSaleDto>> GetUserSalesHistoryAsync(Guid userId, int limit = 5);
}
