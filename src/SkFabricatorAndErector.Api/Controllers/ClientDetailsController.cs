using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/clientdetails")]
public class ClientDetailsController(IClientDetailsService clientDetailsService) : ControllerBase
{
    private readonly IClientDetailsService _clientDetailsService = clientDetailsService;

    [HttpGet]
    public async Task<IActionResult> GetClientDetails()
    {
        var clients = await _clientDetailsService.GetAllClientDetailsAsync();
        return Ok(clients.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClientDetailsById(int id)
    {
        var client = await _clientDetailsService.GetClientDetailsByIdAsync(id);
        if (client == null) return NotFound();
        return Ok(MapToResponse(client));
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> CreateClientDetails([FromForm] CreateClientDetailsRequest request)
    {
        var client = await _clientDetailsService.CreateClientDetailsAsync(request);
        var response = MapToResponse(client);
        return CreatedAtAction(nameof(GetClientDetailsById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> UpdateClientDetails(int id, [FromForm] UpdateClientDetailsRequest request)
    {
        var updatedClient = await _clientDetailsService.UpdateClientDetailsAsync(id, request);
        if (updatedClient == null) return NotFound();
        return Ok(MapToResponse(updatedClient));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> DeleteClientDetails(int id)
    {
        var success = await _clientDetailsService.DeleteClientDetailsAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    private static ClientDetailsResponse MapToResponse(ClientDetails client)
    {
        return new ClientDetailsResponse
        {
            Id = client.Id,
            Name = client.Name,
            ImageUrl = client.ImageUrl,
            ClientUrl = client.ClientUrl
        };
    }
}
