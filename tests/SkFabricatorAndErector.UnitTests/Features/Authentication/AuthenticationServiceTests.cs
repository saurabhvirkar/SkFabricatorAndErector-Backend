using Microsoft.AspNetCore.Identity;
using Moq;
using SkFabricatorAndErector.Application.Contracts.Requests.Auth;
using SkFabricatorAndErector.Application.Features.Authentication;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Features.Authentication;

public class AuthenticationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGeneratorMock = new();
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new AuthenticationService(_userManagerMock.Object, _tokenGeneratorMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest { Email = "admin@skfabricator.com", Password = "WrongPassword" };
        var user = new ApplicationUser { Email = request.Email };

        _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, request.Password)).ReturnsAsync(false);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest { Email = "admin@skfabricator.com", Password = "Admin@123" };
        var user = new ApplicationUser { Email = request.Email, Role = "Admin" };

        _userManagerMock.Setup(u => u.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _tokenGeneratorMock.Setup(t => t.GenerateJwtTokenAsync(user)).ReturnsAsync("jwt-token-123");
        _tokenGeneratorMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token-123");
        _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("jwt-token-123", result.Token);
        Assert.Equal("refresh-token-123", result.RefreshToken);
        Assert.Equal("admin@skfabricator.com", result.Email);
        Assert.Equal("Admin", result.Role);
    }
}
