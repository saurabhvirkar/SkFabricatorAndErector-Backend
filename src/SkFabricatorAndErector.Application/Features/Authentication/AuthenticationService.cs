using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
        var rawRefreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshToken = HashRefreshToken(rawRefreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles?.FirstOrDefault() ?? user.Role;

        return new AuthenticationResponse
        {
            Token = token,
            RefreshToken = rawRefreshToken,
            Email = user.Email,
            Role = userRole
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
        if (user == null)
        {
            return null;
        }

        var incomingHash = HashRefreshToken(request.RefreshToken);

        // Reuse detection / invalid token check
        if (user.RefreshToken != incomingHash || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            // If an invalid or previously used refresh token is presented, revoke the user's tokens family (Reuse Detection)
            if (user.RefreshToken != null && user.RefreshToken != incomingHash)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = DateTime.MinValue;
                await _userManager.UpdateAsync(user);
            }
            return null;
        }

        // Token rotation: Issue new token pair
        var newJwtToken = await _tokenGenerator.GenerateJwtTokenAsync(user);
        var newRawRefreshToken = _tokenGenerator.GenerateRefreshToken();

        user.RefreshToken = HashRefreshToken(newRawRefreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles?.FirstOrDefault() ?? user.Role;

        return new AuthenticationResponse
        {
            Token = newJwtToken,
            RefreshToken = newRawRefreshToken,
            Email = user.Email,
            Role = userRole
        };
    }

    private static string HashRefreshToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
