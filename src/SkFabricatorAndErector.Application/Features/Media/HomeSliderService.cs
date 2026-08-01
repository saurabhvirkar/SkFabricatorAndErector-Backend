using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Media;

public class HomeSliderService(IHomeSliderRepository sliderRepository, IPhotoService photoService, HybridCache cache) : IHomeSliderService
{
    private readonly IHomeSliderRepository _sliderRepository = sliderRepository;
    private readonly IPhotoService _photoService = photoService;
    private readonly HybridCache _cache = cache;

    private const string AllSlidersCacheKey = "sliders:all";

    public async Task<IEnumerable<HomeSlider>> GetAllSlidersAsync()
    {
        return await _cache.GetOrCreateAsync(
            AllSlidersCacheKey,
            async ct => (await _sliderRepository.GetAllAsync()).ToList());
    }

    public async Task<HomeSlider> AddSliderAsync(string title, string description, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be null or empty", nameof(file));
        }

        var uploadResult = await _photoService.AddPhotoAsync(file);
        if (!string.IsNullOrEmpty(uploadResult.Error))
        {
            throw new InvalidOperationException($"Image upload failed: {uploadResult.Error}");
        }

        var (width, height) = await _photoService.GetImageDimensionsAsync(file);

        var slider = new HomeSlider
        {
            Title = title,
            Description = description,
            ImageUrl = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            Width = width,
            Height = height
        };

        await _sliderRepository.AddAsync(slider);
        await _cache.RemoveAsync(AllSlidersCacheKey);
        return slider;
    }

    public async Task<bool> DeleteSliderAsync(int id)
    {
        var slider = await _sliderRepository.GetByIdAsync(id);
        if (slider == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(slider.PublicId))
        {
            await _photoService.DeletePhotoAsync(slider.PublicId);
        }

        await _sliderRepository.DeleteAsync(slider);
        await _cache.RemoveAsync(AllSlidersCacheKey);
        return true;
    }
}
