using DrinkTea.DataAccess.Interfaces;

namespace DrinkTea.BL.Services;

/// <summary>
/// 	Сервис для управления справочником чая и складскими остатками.
/// </summary>
public class TeaService(ITeaRepository teaRepo)
{
    /// <summary>
    /// 	Получает актуальное состояние склада для всех позиций.
    /// </summary>
    public async Task<IEnumerable<dynamic>> GetFullInventoryAsync()
    {
        return await teaRepo.GetInventoryAsync();
    }
}