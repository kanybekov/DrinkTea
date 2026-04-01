using DrinkTea.Domain.Common;
using System.Data;

namespace DrinkTea.DataAccess.Interfaces;

/// <summary>
/// 	Интерфейс для регистрации розничных продаж (Retail) в базе данных.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// 	Записывает факт продажи товара в таблицу Sales.
    /// </summary>
    /// <param name="teaId">		Идентификатор проданного чая. </param>
    /// <param name="userId">		ID клиента (может быть null для анонимов). </param>
    /// <param name="grams">		Вес проданного чая в граммах. </param>
    /// <param name="totalCost">	Итоговая стоимость по розничной цене. </param>
    /// <param name="method">		Метод оплаты (Internal/Cash/Card). </param>
    /// <param name="transaction">	Активная SQL-транзакция. </param>
    Task CreateSaleAsync(Guid teaId, Guid? userId, decimal grams, decimal totalCost, PaymentMethod method, Guid staffId, IDbTransaction transaction);
}