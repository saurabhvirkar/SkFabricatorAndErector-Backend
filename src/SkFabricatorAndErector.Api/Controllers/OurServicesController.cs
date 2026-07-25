using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/ourservices")]
public class OurServicesController(IOurServiceService ourServiceService) : ControllerBase
{
    private readonly IOurServiceService _ourServiceService = ourServiceService;

    [HttpGet]
    public async Task<IActionResult> GetServices()
    {
        var services = await _ourServiceService.GetAllServicesAsync();
        return Ok(services.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceById(int id)
    {
        var service = await _ourServiceService.GetServiceByIdAsync(id);
        if (service == null) return NotFound();
        return Ok(MapToResponse(service));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateService([FromForm] CreateOurServiceRequest request)
    {
        var service = await _ourServiceService.CreateServiceAsync(request);
        var response = MapToResponse(service);
        return CreatedAtAction(nameof(GetServiceById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateService(int id, [FromForm] UpdateOurServiceRequest request)
    {
        var updatedService = await _ourServiceService.UpdateServiceAsync(id, request);
        if (updatedService == null) return NotFound();
        return Ok(MapToResponse(updatedService));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var success = await _ourServiceService.DeleteServiceAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    private static OurServiceResponse MapToResponse(OurService service)
    {
        return new OurServiceResponse
        {
            Id = service.Id,
            Name = service.Name,
            Summary = service.Summary,
            Description = service.Description,
            ImageUrl = service.ImageUrl
        };
    }
}
