using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Responses.Media;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class PhotoControllerTests
{
    private readonly Mock<IPhotoRepository> _repositoryMock = new();
    private readonly Mock<IPhotoService> _photoServiceMock = new();
    private readonly PhotoController _controller;

    public PhotoControllerTests()
    {
        _controller = new PhotoController(_repositoryMock.Object, _photoServiceMock.Object);
    }

    [Fact]
    public async Task GetPhotos_ShouldReturnOk_WithPhotoList()
    {
        // Arrange
        var photos = new List<Photo>
        {
            new() { Id = 1, Url = "http://example.com/1.jpg", Category = "Steel" }
        };
        _repositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Photo, bool>>>()))
                       .ReturnsAsync(photos);

        // Act
        var result = await _controller.GetPhotos();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<PhotoResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task DeletePhoto_ShouldReturnNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Photo?)null);

        // Act
        var result = await _controller.DeletePhoto(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
