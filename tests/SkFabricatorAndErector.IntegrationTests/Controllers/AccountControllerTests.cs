using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Common;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Requests.Auth;
using SkFabricatorAndErector.Application.Contracts.Responses.Auth;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class AccountControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly AccountController _controller;

    public AccountControllerTests()
    {
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _controller = new AccountController(_authServiceMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
        var request = new LoginRequest { Email = "user@example.com", Password = "bad" };
        _authServiceMock.Setup(a => a.LoginAsync(request)).ReturnsAsync((AuthenticationResponse?)null);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(unauthorizedResult.Value);
        Assert.Equal(401, apiResponse.StatusCode);
        Assert.Equal("Invalid credentials.", apiResponse.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnOk_WithApiResponseWrappingAuthenticationResponse()
    {
        // Arrange
        var request = new LoginRequest { Email = "admin@skfabricator.com", Password = "Admin@123" };
        var expectedResponse = new AuthenticationResponse
        {
            Token = "valid-jwt",
            RefreshToken = "valid-refresh",
            Email = request.Email,
            Role = "Admin"
        };
        _authServiceMock.Setup(a => a.LoginAsync(request)).ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.LoginAsync(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(200, apiResponse.StatusCode);
        Assert.Equal("Login successful.", apiResponse.Message);
        var authData = Assert.IsType<AuthenticationResponse>(apiResponse.Data);
        Assert.Equal("valid-jwt", authData.Token);
    }
}
