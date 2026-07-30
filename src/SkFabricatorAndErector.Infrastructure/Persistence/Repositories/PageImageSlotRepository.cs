using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence.Repositories;

public class PageImageSlotRepository : GenericRepository<PageImageSlot>, IPageImageSlotRepository
{
    public PageImageSlotRepository(ApplicationDbContext context, ILogger<GenericRepository<PageImageSlot>>? logger = null)
        : base(context, logger)
    {
    }

    public async Task<PageImageSlot?> GetBySlotKeyAsync(string slotKey)
    {
        return await _context.PageImageSlots.FirstOrDefaultAsync(s => s.SlotKey.ToLower() == slotKey.ToLower());
    }

    public async Task<IEnumerable<PageImageSlot>> GetAllSlotsAsync()
    {
        return await _context.PageImageSlots.OrderBy(s => s.PageName).ThenBy(s => s.SectionName).ToListAsync();
    }
}
