using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class OurServicesControllerTests
{
    private readonly Mock<IOurServiceService> _serviceMock = new();
    private readonly OurServicesController _controller;

    public OurServicesControllerTests()
    {
        _controller = new OurServicesController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetServices_ShouldReturnOk_WithList()
    {
        // Arrange
        var services = new List<OurService>
        {
            new() { Id = 1, Name = "Fabrication", Summary = "Custom metal design" }
        };
        _serviceMock.Setup(s => s.GetAllServicesAsync()).ReturnsAsync(services);

        // Act
        var result = await _controller.GetServices();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<OurServiceResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetServiceById_ShouldReturnNotFound_WhenServiceDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetServiceByIdAsync(999)).ReturnsAsync((OurService?)null);

        // Act
        var result = await _controller.GetServiceById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
