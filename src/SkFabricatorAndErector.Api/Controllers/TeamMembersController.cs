using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/teammembers")]
public class TeamMembersController(ITeamMemberService teamMemberService) : ControllerBase
{
    private readonly ITeamMemberService _teamMemberService = teamMemberService;

    [HttpGet]
    public async Task<IActionResult> GetTeamMembers()
    {
        var members = await _teamMemberService.GetAllTeamMembersAsync();
        return Ok(members.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeamMemberById(int id)
    {
        var member = await _teamMemberService.GetTeamMemberByIdAsync(id);
        if (member == null) return NotFound();
        return Ok(MapToResponse(member));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateTeamMember([FromForm] CreateTeamMemberRequest request)
    {
        var member = await _teamMemberService.CreateTeamMemberAsync(request);
        var response = MapToResponse(member);
        return CreatedAtAction(nameof(GetTeamMemberById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateTeamMember(int id, [FromForm] UpdateTeamMemberRequest request)
    {
        var updatedMember = await _teamMemberService.UpdateTeamMemberAsync(id, request);
        if (updatedMember == null) return NotFound();
        return Ok(MapToResponse(updatedMember));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteTeamMember(int id)
    {
        var success = await _teamMemberService.DeleteTeamMemberAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    private static TeamMemberResponse MapToResponse(TeamMember member)
    {
        return new TeamMemberResponse
        {
            Id = member.Id,
            Name = member.Name,
            Role = member.Role,
            ImageUrl = member.ImageUrl,
            Email = member.Email,
            LinkedInUrl = member.LinkedInUrl,
            Details = member.Details,
            PublicId = member.PublicId
        };
    }
}
