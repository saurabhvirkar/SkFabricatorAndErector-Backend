using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Persistence;

public interface IPageImageSlotRepository : IGenericRepository<PageImageSlot>
{
    Task<PageImageSlot?> GetBySlotKeyAsync(string slotKey);
    Task<IEnumerable<PageImageSlot>> GetAllSlotsAsync();
}
