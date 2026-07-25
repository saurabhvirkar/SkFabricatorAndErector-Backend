using Moq;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Features.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Features.Catalog;

public class ClientDetailsServiceTests
{
    private readonly Mock<IClientDetailsRepository> _repositoryMock = new();
    private readonly Mock<IPhotoService> _photoServiceMock = new();
    private readonly ClientDetailsService _sut;

    public ClientDetailsServiceTests()
    {
        _sut = new ClientDetailsService(_repositoryMock.Object, _photoServiceMock.Object);
    }

    [Fact]
    public async Task GetAllClientDetailsAsync_ShouldReturnList()
    {
        // Arrange
        var clients = new List<ClientDetails>
        {
            new() { Id = 1, Name = "Acme Corp", ClientUrl = "https://acme.com" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);

        // Act
        var result = await _sut.GetAllClientDetailsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Acme Corp", result.First().Name);
    }

    [Fact]
    public async Task CreateClientDetailsAsync_ShouldSaveClient()
    {
        // Arrange
        var request = new CreateClientDetailsRequest
        {
            Name = "Global Steel Inc",
            ClientUrl = "https://globalsteel.com"
        };

        // Act
        var result = await _sut.CreateClientDetailsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Global Steel Inc", result.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ClientDetails>()), Times.Once);
    }

    [Fact]
    public async Task DeleteClientDetailsAsync_ShouldReturnFalse_WhenNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ClientDetails?)null);

        // Act
        var result = await _sut.DeleteClientDetailsAsync(99);

        // Assert
        Assert.False(result);
    }
}
