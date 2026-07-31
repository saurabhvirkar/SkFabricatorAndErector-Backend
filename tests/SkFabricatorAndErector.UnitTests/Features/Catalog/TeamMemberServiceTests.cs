using Moq;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Features.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.UnitTests.Features.Catalog;

public class TeamMemberServiceTests
{
    private readonly Mock<ITeamMemberRepository> _repositoryMock = new();
    private readonly Mock<IPhotoService> _photoServiceMock = new();
    private readonly TeamMemberService _sut;

    public TeamMemberServiceTests()
    {
        _sut = new TeamMemberService(_repositoryMock.Object, _photoServiceMock.Object);
    }

    [Fact]
    public async Task GetTeamMemberByIdAsync_ShouldReturnMember_WhenFound()
    {
        // Arrange
        var member = new TeamMember { Id = 1, Name = "John Chief", Role = "CEO" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(member);

        // Act
        var result = await _sut.GetTeamMemberByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John Chief", result.Name);
    }

    [Fact]
    public async Task CreateTeamMemberAsync_ShouldSaveMember()
    {
        // Arrange
        var request = new CreateTeamMemberRequest
        {
            Name = "Jane Lead",
            Role = "Project Engineer",
            Email = "jane@example.com"
        };

        // Act
        var result = await _sut.CreateTeamMemberAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jane Lead", result.Name);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<TeamMember>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTeamMemberAsync_ShouldDeletePhotoAndEntity_WhenPhotoExists()
    {
        // Arrange
        var member = new TeamMember { Id = 3, Name = "Bob", PublicId = "team-pub-3" };
        _repositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(member);

        // Act
        var result = await _sut.DeleteTeamMemberAsync(3);

        // Assert
        Assert.True(result);
        _photoServiceMock.Verify(p => p.DeletePhotoAsync("team-pub-3"), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(member), Times.Once);
    }
}
