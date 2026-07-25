using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Persistence;

public interface IPhotoRepository : IGenericRepository<Photo>
{
    Task<IEnumerable<Photo>> GetAboutSliderPhotosAsync();
}
