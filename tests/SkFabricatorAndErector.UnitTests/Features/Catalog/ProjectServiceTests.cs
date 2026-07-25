using Moq;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Features.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Features.Catalog;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IPhotoService> _photoServiceMock = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _sut = new ProjectService(_projectRepositoryMock.Object, _photoServiceMock.Object);
    }

    [Fact]
    public async Task CreateProjectAsync_ShouldSaveProject_WithoutFile()
    {
        // Arrange
        var request = new CreateProjectRequest { Title = "Steel Bridge", Category = "Heavy Fabrication" };

        // Act
        var project = await _sut.CreateProjectAsync(request);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Steel Bridge", project.Title);
        _projectRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProjectAsync_ShouldDeletePhotoFromCloudinary_AndDatabase()
    {
        // Arrange
        var project = new Project { Id = 10, PublicId = "proj-pub-1" };
        _projectRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(project);

        // Act
        var result = await _sut.DeleteProjectAsync(10);

        // Assert
        Assert.True(result);
        _photoServiceMock.Verify(p => p.DeletePhotoAsync("proj-pub-1"), Times.Once);
        _projectRepositoryMock.Verify(r => r.DeleteAsync(project), Times.Once);
    }
}
