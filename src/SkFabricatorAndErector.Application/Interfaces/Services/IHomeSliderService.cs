using Microsoft.AspNetCore.Http;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IHomeSliderService
{
    Task<IEnumerable<HomeSlider>> GetAllSlidersAsync();
    Task<HomeSlider> AddSliderAsync(string title, string description, IFormFile file);
    Task<bool> DeleteSliderAsync(int id);
}
