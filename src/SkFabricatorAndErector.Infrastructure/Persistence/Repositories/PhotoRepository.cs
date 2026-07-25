using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence.Repositories;

public class PhotoRepository : GenericRepository<Photo>, IPhotoRepository
{
    public PhotoRepository(ApplicationDbContext context, ILogger<GenericRepository<Photo>>? logger = null)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Photo>> GetAboutSliderPhotosAsync()
    {
        return await _context.Photos.Where(p => p.IsAboutSlider).ToListAsync();
    }
}
