using Microsoft.AspNetCore.Http;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<bool> DeletePhotoAsync(string publicId);
    Task<(int Width, int Height)> GetImageDimensionsAsync(IFormFile file);
}
