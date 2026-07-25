using Moq;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Features.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Features.Catalog;

public class OurServiceServiceTests
{
    private readonly Mock<IOurServiceRepository> _repositoryMock = new();
    private readonly Mock<IPhotoService> _photoServiceMock = new();
    private readonly OurServiceService _sut;

    public OurServiceServiceTests()
    {
        _sut = new OurServiceService(_repositoryMock.Object, _photoServiceMock.Object);
    }

    [Fact]
    public async Task GetAllServicesAsync_ShouldReturnAllServices()
    {
        // Arrange
        var services = new List<OurService>
        {
            new() { Id = 1, Name = "Structural Fabrication", Summary = "Heavy steel works" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(services);

        // Act
        var result = await _sut.GetAllServicesAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Structural Fabrication", result.First().Name);
    }

    [Fact]
    public async Task CreateServiceAsync_ShouldAddServiceToRepository()
    {
        // Arrange
        var request = new CreateOurServiceRequest
        {
            Name = "Piping Installation",
            Summary = "High pressure piping",
            Description = "Full installation services"
        };

        // Act
        var result = await _sut.CreateServiceAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Piping Installation", result.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<OurService>()), Times.Once);
    }

    [Fact]
    public async Task DeleteServiceAsync_ShouldReturnTrue_WhenServiceExists()
    {
        // Arrange
        var service = new OurService { Id = 5, Name = "Rigging" };
        _repositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(service);

        // Act
        var result = await _sut.DeleteServiceAsync(5);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(service), Times.Once);
    }
}
