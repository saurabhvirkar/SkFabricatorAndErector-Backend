using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class TeamMembersControllerTests
{
    private readonly Mock<ITeamMemberService> _serviceMock = new();
    private readonly TeamMembersController _controller;

    public TeamMembersControllerTests()
    {
        _controller = new TeamMembersController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetTeamMembers_ShouldReturnOk_WithList()
    {
        // Arrange
        var members = new List<TeamMember>
        {
            new() { Id = 1, Name = "Alice Engineer", Role = "Senior Fabricator" }
        };
        _serviceMock.Setup(s => s.GetAllTeamMembersAsync()).ReturnsAsync(members);

        // Act
        var result = await _controller.GetTeamMembers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<TeamMemberResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetTeamMemberById_ShouldReturnNotFound_WhenMemberDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetTeamMemberByIdAsync(999)).ReturnsAsync((TeamMember?)null);

        // Act
        var result = await _controller.GetTeamMemberById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
