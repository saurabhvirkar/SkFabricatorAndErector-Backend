using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/page-images")]
public class PageImagesController(IPageImageSlotRepository slotRepository, IPhotoService photoService) : ControllerBase
{
    private readonly IPageImageSlotRepository _slotRepository = slotRepository;
    private readonly IPhotoService _photoService = photoService;

    [HttpGet]
    public async Task<IActionResult> GetAllSlots()
    {
        var slots = await _slotRepository.GetAllSlotsAsync();
        var response = slots.Select(MapToResponse);
        return Ok(response);
    }

    [HttpGet("{slotKey}")]
    public async Task<IActionResult> GetBySlotKey(string slotKey)
    {
        var slot = await _slotRepository.GetBySlotKeyAsync(slotKey);
        if (slot == null) return NotFound(new { message = $"Slot key '{slotKey}' not found." });
        return Ok(MapToResponse(slot));
    }

    [HttpGet("admin")]
    [Authorize]
    public async Task<IActionResult> GetAdminSlotRegistry()
    {
        var slots = await _slotRepository.GetAllSlotsAsync();
        var response = slots.Select(MapToResponse);
        return Ok(response);
    }

    [HttpPost("{slotKey}")]
    [Authorize]
    public async Task<IActionResult> UploadSlotImage(string slotKey, [FromForm] IFormFile file, [FromForm] string? altText)
    {
        var slot = await _slotRepository.GetBySlotKeyAsync(slotKey);
        if (slot == null)
        {
            // Auto-register slot if it doesn't exist yet
            var parts = slotKey.Split('.');
            var page = parts.Length > 0 ? char.ToUpper(parts[0][0]) + parts[0][1..] : "General";
            var section = parts.Length > 1 ? char.ToUpper(parts[1][0]) + parts[1][1..] : "Section";
            var label = parts.Length > 2 ? parts[2] : slotKey;

            slot = new PageImageSlot
            {
                SlotKey = slotKey.ToLower(),
                PageName = page,
                SectionName = section,
                Label = label,
                UpdatedAtUtc = DateTime.UtcNow
            };
            await _slotRepository.AddAsync(slot);
            await _slotRepository.SaveChangesAsync();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please select a valid image file to upload." });
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            return BadRequest(new { message = "Invalid file type. Only .jpg, .jpeg, .png, and .webp images are allowed." });
        }

        if (file.Length > 8 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size exceeds the 8 MB maximum limit." });
        }

        // Delete old asset if exists
        if (!string.IsNullOrEmpty(slot.CloudinaryPublicId))
        {
            await _photoService.DeletePhotoAsync(slot.CloudinaryPublicId);
        }

        // Upload new image to Cloudinary
        var uploadResult = await _photoService.AddPhotoAsync(file);
        if (uploadResult.Error != null)
        {
            return BadRequest(new { message = uploadResult.Error });
        }

        slot.ImageUrl = uploadResult.Url;
        slot.CloudinaryPublicId = uploadResult.PublicId;
        if (!string.IsNullOrWhiteSpace(altText))
        {
            slot.AltText = altText;
        }
        slot.UpdatedAtUtc = DateTime.UtcNow;

        await _slotRepository.UpdateAsync(slot);
        await _slotRepository.SaveChangesAsync();

        return Ok(MapToResponse(slot));
    }

    [HttpDelete("{slotKey}")]
    [Authorize]
    public async Task<IActionResult> DeleteSlotImage(string slotKey)
    {
        var slot = await _slotRepository.GetBySlotKeyAsync(slotKey);
        if (slot == null) return NotFound(new { message = $"Slot key '{slotKey}' not found." });

        if (!string.IsNullOrEmpty(slot.CloudinaryPublicId))
        {
            await _photoService.DeletePhotoAsync(slot.CloudinaryPublicId);
        }

        slot.ImageUrl = null;
        slot.CloudinaryPublicId = null;
        slot.UpdatedAtUtc = DateTime.UtcNow;

        await _slotRepository.UpdateAsync(slot);
        await _slotRepository.SaveChangesAsync();

        return Ok(MapToResponse(slot));
    }

    private static object MapToResponse(PageImageSlot slot)
    {
        return new
        {
            slot.Id,
            slot.SlotKey,
            slot.PageName,
            slot.SectionName,
            slot.Label,
            slot.ImageUrl,
            slot.AltText,
            slot.UpdatedAtUtc,
            HasImage = !string.IsNullOrEmpty(slot.ImageUrl)
        };
    }
}
