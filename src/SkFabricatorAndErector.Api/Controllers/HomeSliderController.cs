using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Media;
using SkFabricatorAndErector.Application.Contracts.Responses.Media;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/homeslider")]
public class HomeSliderController(IHomeSliderService homeSliderService) : ControllerBase
{
    private readonly IHomeSliderService _homeSliderService = homeSliderService;

    [HttpGet]
    public async Task<IActionResult> GetHomeSliders()
    {
        var sliders = await _homeSliderService.GetAllSlidersAsync();
        return Ok(sliders.Select(MapToResponse));
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> AddHomeSlider([FromForm] AddHomeSliderRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File is required.");
        }

        var slider = await _homeSliderService.AddSliderAsync(request.Title, request.Description, request.File);
        return Ok(MapToResponse(slider));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> DeleteHomeSlider(int id)
    {
        var success = await _homeSliderService.DeleteSliderAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    private static HomeSliderResponse MapToResponse(HomeSlider slider)
    {
        return new HomeSliderResponse
        {
            Id = slider.Id,
            Title = slider.Title,
            Description = slider.Description,
            ImageUrl = slider.ImageUrl,
            PublicId = slider.PublicId,
            Width = slider.Width,
            Height = slider.Height
        };
    }
}
