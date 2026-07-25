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

    public CloudinaryPhotoService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ApplicationImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ApplicationImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "sk-fabricator"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                uploadResult.Error = result.Error.Message;
                return uploadResult;
            }

            uploadResult.Url = result.SecureUrl.ToString();
            uploadResult.PublicId = result.PublicId;
            uploadResult.Width = result.Width;
            uploadResult.Height = result.Height;
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
