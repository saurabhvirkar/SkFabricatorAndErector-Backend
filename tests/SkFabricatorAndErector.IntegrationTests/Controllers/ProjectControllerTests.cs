using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class ProjectControllerTests
{
    private readonly Mock<IProjectService> _serviceMock = new();
    private readonly ProjectController _controller;

    public ProjectControllerTests()
    {
        _controller = new ProjectController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetProjects_ShouldReturnOk_WithProjectList()
    {
        // Arrange
        var projects = new List<Project>
        {
            new() { Id = 1, Title = "Warehouse Frame", Category = "Industrial" }
        };
        _serviceMock.Setup(s => s.GetAllProjectsAsync()).ReturnsAsync(projects);

        // Act
        var result = await _controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ProjectResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetProjectById_ShouldReturnNotFound_WhenProjectDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetProjectByIdAsync(999)).ReturnsAsync((Project?)null);

        // Act
        var result = await _controller.GetProjectById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
