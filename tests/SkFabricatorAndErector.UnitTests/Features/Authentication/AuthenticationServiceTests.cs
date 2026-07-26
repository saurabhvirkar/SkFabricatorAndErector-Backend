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
    private readonly Mock<IOtpService> _otpServiceMock = new();
    private readonly AuthenticationService _sut;

    public AuthenticationServiceTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new AuthenticationService(_userManagerMock.Object, _tokenGeneratorMock.Object, _otpServiceMock.Object);
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

    [Fact]
    public async Task ChangePasswordAsync_ShouldSucceed_WhenValid()
    {
        // Arrange
        var userId = "user-123";
        var user = new ApplicationUser { Id = userId, Email = "user@test.com" };
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword@123",
            NewPassword = "NewPassword@123",
            ConfirmNewPassword = "NewPassword@123"
        };

        _userManagerMock.Setup(u => u.FindByIdAsync(userId)).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ChangePasswordAsync(userId, request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldFail_WhenNewPasswordMatchesCurrent()
    {
        // Arrange
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "SamePassword@123",
            NewPassword = "SamePassword@123",
            ConfirmNewPassword = "SamePassword@123"
        };

        // Act
        var result = await _sut.ChangePasswordAsync("user-123", request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("New password cannot be the same as the current password.", result.Errors);
    }
}
