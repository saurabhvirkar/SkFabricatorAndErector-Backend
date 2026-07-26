using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/ourservices")]
[Route("api/services")]
public class OurServicesController(IOurServiceService serviceService) : ControllerBase
{
    private readonly IOurServiceService _serviceService = serviceService;

    [HttpGet]
    public async Task<IActionResult> GetServices()
    {
        var services = await _serviceService.GetAllServicesAsync();
        return Ok(services.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceById(int id)
    {
        var service = await _serviceService.GetServiceByIdAsync(id);
        if (service == null) return NotFound();
        return Ok(MapToResponse(service));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Services.Create)]
    public async Task<IActionResult> CreateService([FromForm] CreateOurServiceRequest request)
    {
        var service = await _serviceService.CreateServiceAsync(request);
        var response = MapToResponse(service);
        return CreatedAtAction(nameof(GetServiceById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Services.Update)]
    public async Task<IActionResult> UpdateService(int id, [FromForm] UpdateOurServiceRequest request)
    {
        var updatedService = await _serviceService.UpdateServiceAsync(id, request);
        if (updatedService == null) return NotFound();
        return Ok(MapToResponse(updatedService));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Services.Delete)]
    public async Task<IActionResult> DeleteService(int id)
    {
        var success = await _serviceService.DeleteServiceAsync(id);
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
