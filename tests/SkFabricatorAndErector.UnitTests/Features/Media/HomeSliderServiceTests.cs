using Microsoft.AspNetCore.Http;
using Moq;
using SkFabricatorAndErector.Application.Features.Media;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Features.Media;

public class HomeSliderServiceTests
{
    private readonly Mock<IHomeSliderRepository> _sliderRepositoryMock = new();
    private readonly Mock<IPhotoService> _photoServiceMock = new();
    private readonly HomeSliderService _sut;

    public HomeSliderServiceTests()
    {
        _sut = new HomeSliderService(_sliderRepositoryMock.Object, _photoServiceMock.Object);
    }

    [Fact]
    public async Task GetAllSlidersAsync_ShouldReturnAllSlidersFromRepository()
    {
        // Arrange
        var sliders = new List<HomeSlider>
        {
            new() { Id = 1, Title = "Slide 1" },
            new() { Id = 2, Title = "Slide 2" }
        };
        _sliderRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sliders);

        // Act
        var result = await _sut.GetAllSlidersAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddSliderAsync_ShouldUploadFile_AndSaveSlider()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);

        _photoServiceMock.Setup(p => p.AddPhotoAsync(fileMock.Object))
            .ReturnsAsync(new ImageUploadResult { Url = "http://res.cloudinary.com/img.jpg", PublicId = "pub1" });
        _photoServiceMock.Setup(p => p.GetImageDimensionsAsync(fileMock.Object))
            .ReturnsAsync((800, 600));

        // Act
        var slider = await _sut.AddSliderAsync("Title", "Desc", fileMock.Object);

        // Assert
        Assert.NotNull(slider);
        Assert.Equal("Title", slider.Title);
        Assert.Equal("http://res.cloudinary.com/img.jpg", slider.ImageUrl);
        Assert.Equal(800, slider.Width);
        Assert.Equal(600, slider.Height);
        _sliderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<HomeSlider>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSliderAsync_ShouldDeletePhotoFromCloudinary_AndDatabase()
    {
        // Arrange
        var slider = new HomeSlider { Id = 5, PublicId = "pub-5" };
        _sliderRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(slider);

        // Act
        var result = await _sut.DeleteSliderAsync(5);

        // Assert
        Assert.True(result);
        _photoServiceMock.Verify(p => p.DeletePhotoAsync("pub-5"), Times.Once);
        _sliderRepositoryMock.Verify(r => r.DeleteAsync(slider), Times.Once);
    }
}
