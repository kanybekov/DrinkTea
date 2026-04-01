using DrinkTea.Domain.Entities;
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
    Task<Visit> CreateAsync(Guid? userId);

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

}
