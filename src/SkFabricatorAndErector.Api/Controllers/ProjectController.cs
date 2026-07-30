using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/project")]
[Route("api/projects")]
public class ProjectController(IProjectService projectService) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;

    [HttpGet]
    public async Task<IActionResult> GetProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects.Select(MapToResponse));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(MapToResponse(project));
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetProjectsByCategory(string category)
    {
        var projects = await _projectService.GetProjectsByCategoryAsync(category);
        return Ok(projects.Select(MapToResponse));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Projects.Create)]
    public async Task<IActionResult> CreateProject([FromForm] CreateProjectRequest request)
    {
        var project = await _projectService.CreateProjectAsync(request);
        var response = MapToResponse(project);
        return CreatedAtAction(nameof(GetProjectById), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.Projects.Update)]
    public async Task<IActionResult> UpdateProject(int id, [FromForm] UpdateProjectRequest request)
    {
        var updatedProject = await _projectService.UpdateProjectAsync(id, request);
        if (updatedProject == null) return NotFound();
        return Ok(MapToResponse(updatedProject));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Projects.Delete)]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var success = await _projectService.DeleteProjectAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    private static ProjectResponse MapToResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Title = project.Title,
            Description = project.Description,
            Image = project.Image,
            Category = project.Category,
            PublicId = project.PublicId,
            CategoryLabel = project.CategoryLabel,
            Client = project.Client,
            PhotoPlaceholder = project.PhotoPlaceholder
        };
    }
}
