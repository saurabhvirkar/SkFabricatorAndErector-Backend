using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence.Repositories;

public class HomeSliderRepository : GenericRepository<HomeSlider>, IHomeSliderRepository
{
    public HomeSliderRepository(ApplicationDbContext context, ILogger<GenericRepository<HomeSlider>>? logger = null)
        : base(context, logger)
    {
    }
}
