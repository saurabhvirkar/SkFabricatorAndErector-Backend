using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/homeslider")]
[Route("api/homesliders")]
[Route("api/sliders")]
public class HomeSliderController(IHomeSliderService homeSliderService) : ControllerBase
{
    private readonly IHomeSliderService _homeSliderService = homeSliderService;

    [HttpGet]
    public async Task<IActionResult> GetSliders()
    {
        var sliders = await _homeSliderService.GetAllSlidersAsync();
        return Ok(sliders);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.HomeSlider.Create)]
    public async Task<IActionResult> CreateSlider([FromForm] CreateHomeSliderRequest request)
    {
        var slider = await _homeSliderService.AddSliderAsync(request.Title, request.Subtitle, request.File);
        return Ok(slider);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.HomeSlider.Delete)]
    public async Task<IActionResult> DeleteSlider(int id)
    {
        var success = await _homeSliderService.DeleteSliderAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
