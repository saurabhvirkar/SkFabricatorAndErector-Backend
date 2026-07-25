using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Media;
using SkFabricatorAndErector.Application.Contracts.Responses.Media;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/photos")]
public class PhotoController(IPhotoRepository photoRepository, IPhotoService photoService) : ControllerBase
{
    private readonly IPhotoRepository _photoRepository = photoRepository;
    private readonly IPhotoService _photoService = photoService;

    [HttpGet]
    public async Task<IActionResult> GetPhotos()
    {
        var photos = await _photoRepository.FindAsync(p => !p.IsAboutSlider);
        return Ok(photos.Select(MapToResponse));
    }

    [HttpGet("about-slider")]
    public async Task<IActionResult> GetAboutSliderPhotos()
    {
        var photos = await _photoRepository.GetAboutSliderPhotosAsync();
        return Ok(photos.Select(MapToResponse));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddPhoto([FromForm] AddPhotoRequest request)
    {
        var file = request.File;
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var result = await _photoService.AddPhotoAsync(file);

        if (!string.IsNullOrEmpty(result.Error))
        {
            return BadRequest(result.Error);
        }

        var (width, height) = await _photoService.GetImageDimensionsAsync(file);

        var photo = new Photo
        {
            Url = result.Url,
            PublicId = result.PublicId,
            Category = request.Category,
            IsAboutSlider = request.IsAboutSlider,
            Width = width,
            Height = height
        };

        await _photoRepository.AddAsync(photo);

        return Ok(MapToResponse(photo));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _photoRepository.GetByIdAsync(id);
        if (photo == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(photo.PublicId))
        {
            await _photoService.DeletePhotoAsync(photo.PublicId);
        }

        await _photoRepository.DeleteAsync(photo);

        return NoContent();
    }

    [HttpDelete("about-slider/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteAboutSliderPhoto(int id)
    {
        var photo = await _photoRepository.GetByIdAsync(id);
        if (photo == null || !photo.IsAboutSlider)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(photo.PublicId))
        {
            await _photoService.DeletePhotoAsync(photo.PublicId);
        }

        await _photoRepository.DeleteAsync(photo);

        return NoContent();
    }

    private static PhotoResponse MapToResponse(Photo photo)
    {
        return new PhotoResponse
        {
            Id = photo.Id,
            Url = photo.Url,
            IsMain = photo.IsMain,
            PublicId = photo.PublicId,
            IsAboutSlider = photo.IsAboutSlider,
            Category = photo.Category,
            Width = photo.Width,
            Height = photo.Height
        };
    }
}
