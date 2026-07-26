using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Responses.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class ClientDetailsControllerTests
{
    private readonly Mock<IClientDetailsService> _serviceMock = new();
    private readonly ClientDetailsController _controller;

    public ClientDetailsControllerTests()
    {
        _controller = new ClientDetailsController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetClientDetails_ShouldReturnOk_WithList()
    {
        // Arrange
        var clients = new List<ClientDetails>
        {
            new() { Id = 1, Name = "Mega Corp", ClientUrl = "https://megacorp.com" }
        };
        _serviceMock.Setup(s => s.GetAllClientDetailsAsync()).ReturnsAsync(clients);

        // Act
        var result = await _controller.GetClients();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ClientDetailsResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task GetClientDetailsById_ShouldReturnNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetClientDetailsByIdAsync(999)).ReturnsAsync((ClientDetails?)null);

        // Act
        var result = await _controller.GetClientById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
