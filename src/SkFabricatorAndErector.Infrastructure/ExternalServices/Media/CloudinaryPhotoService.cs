using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SkFabricatorAndErector.Application.Interfaces.Services;
using ApplicationImageUploadResult = SkFabricatorAndErector.Application.Interfaces.Services.ImageUploadResult;

namespace SkFabricatorAndErector.Infrastructure.ExternalServices.Media;

public class CloudinaryPhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _config;

    public CloudinaryPhotoService(IOptions<CloudinarySettings> config)
    {
        _config = config.Value;
        var acc = new Account(
            _config.CloudName,
            _config.ApiKey,
            _config.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ApplicationImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ApplicationImageUploadResult();

        var validationError = FileUploadValidator.ValidateImageFile(file);
        if (validationError != null)
        {
            uploadResult.Error = validationError;
            return uploadResult;
        }

        if (file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName);
            var safeServerFileName = $"{Guid.NewGuid():N}{ext}";

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(safeServerFileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = string.IsNullOrWhiteSpace(_config.Folder) ? "sk-fabricator" : _config.Folder
            };

            try
            {
                if (!string.IsNullOrWhiteSpace(_config.CloudName) && !_config.CloudName.StartsWith("REPLACE_WITH_"))
                {
                    var result = await _cloudinary.UploadAsync(uploadParams);
                    if (result.Error == null && result.SecureUrl != null)
                    {
                        uploadResult.Url = result.SecureUrl.ToString();
                        uploadResult.PublicId = result.PublicId;
                        uploadResult.Width = result.Width;
                        uploadResult.Height = result.Height;
                        return uploadResult;
                    }
                }
            }
            catch
            {
                // Fallback to local storage
            }

            // Local Disk Media Storage Fallback
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, safeServerFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var dimensions = await GetImageDimensionsAsync(file);

            uploadResult.Url = $"/uploads/{safeServerFileName}";
            uploadResult.PublicId = safeServerFileName;
            uploadResult.Width = dimensions.Width;
            uploadResult.Height = dimensions.Height;
        }

        return uploadResult;
    }

    public async Task<bool> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }

    public async Task<(int Width, int Height)> GetImageDimensionsAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var image = await Image.LoadAsync(stream);
        return (image.Width, image.Height);
    }
}
