using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SkFabricatorAndErector.Application.Contracts.Requests.Auth;
using SkFabricatorAndErector.Application.Contracts.Responses.Auth;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Authentication;

public class AuthenticationService(UserManager<ApplicationUser> userManager, IJwtTokenGenerator tokenGenerator) : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IJwtTokenGenerator _tokenGenerator = tokenGenerator;

    public async Task<AuthenticationResponse?> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return null;
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return null;
        }

        var token = await _tokenGenerator.GenerateJwtTokenAsync(user);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthenticationResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthenticationResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.AccessToken) || string.IsNullOrEmpty(request.RefreshToken))
        {
            return null;
        }

        var principal = _tokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return null;
        }

        var username = principal.Identity?.Name ?? principal.FindFirstValue(ClaimTypes.Name) ?? principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(username))
        {
            return null;
        }

        var user = await _userManager.FindByNameAsync(username) ?? await _userManager.FindByEmailAsync(username);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        var newJwtToken = await _tokenGenerator.GenerateJwtTokenAsync(user);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new AuthenticationResponse
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken,
            Email = user.Email,
            Role = user.Role
        };
    }
}
