using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Contracts.Responses.Media;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/photo")]
[Route("api/photos")]
public class PhotoController(IPhotoRepository photoRepository, IPhotoService photoService) : ControllerBase
{
    private readonly IPhotoRepository _photoRepository = photoRepository;
    private readonly IPhotoService _photoService = photoService;

    [HttpGet]
    public async Task<IActionResult> GetPhotos()
    {
        var photos = await _photoRepository.FindAsync(p => p.IsAboutSlider == false);
        return Ok(photos.Select(MapToResponse));
    }

    [HttpGet("about-slider")]
    public async Task<IActionResult> GetAboutSliderPhotos()
    {
        var photos = await _photoRepository.FindAsync(p => p.IsAboutSlider == true);
        return Ok(photos.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhotoById(int id)
    {
        var photo = await _photoRepository.GetByIdAsync(id);
        if (photo == null) return NotFound();
        return Ok(MapToResponse(photo));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Gallery.Create)]
    public async Task<IActionResult> AddPhoto([FromForm] CreatePhotoRequest request)
    {
        var uploadResult = await _photoService.AddPhotoAsync(request.File);
        if (uploadResult.Error != null)
        {
            return BadRequest(uploadResult.Error);
        }

        var photo = new Photo
        {
            Url = uploadResult.Url,
            PublicId = uploadResult.PublicId,
            Category = request.Category ?? "General",
            IsAboutSlider = request.IsAboutSlider,
            Width = uploadResult.Width,
            Height = uploadResult.Height
        };

        await _photoRepository.AddAsync(photo);
        await _photoRepository.SaveChangesAsync();

        var response = MapToResponse(photo);
        return CreatedAtAction(nameof(GetPhotoById), new { id = response.Id }, response);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Gallery.Delete)]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var photo = await _photoRepository.GetByIdAsync(id);
        if (photo == null) return NotFound();

        if (!string.IsNullOrEmpty(photo.PublicId))
        {
            await _photoService.DeletePhotoAsync(photo.PublicId);
        }

        await _photoRepository.DeleteAsync(photo);
        await _photoRepository.SaveChangesAsync();

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
