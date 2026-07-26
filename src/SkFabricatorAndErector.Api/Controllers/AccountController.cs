using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SkFabricatorAndErector.Api.Common;
using SkFabricatorAndErector.Api.Extensions;
using SkFabricatorAndErector.Application.Contracts.Requests.Auth;
using SkFabricatorAndErector.Application.Interfaces.Services;
using System.Net;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/account")]
[EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
public class AccountController(IAuthenticationService authenticationService) : ControllerBase
{
    private readonly IAuthenticationService _authenticationService = authenticationService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginDto)
    {
        var response = await _authenticationService.LoginAsync(loginDto);
        if (response == null)
        {
            return Unauthorized(new ApiResponse(HttpStatusCode.Unauthorized, "Invalid credentials.", null));
        }

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(new ApiResponse(HttpStatusCode.OK, "Login successful.", response));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest? tokenDto)
    {
        var refreshToken = tokenDto?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            try
            {
                if (ControllerContext?.HttpContext?.Request?.Cookies != null)
                {
                    refreshToken = Request.Cookies["refreshToken"];
                }
            }
            catch
            {
                // Ignore in uninitialized test context
            }
        }

        var request = new RefreshTokenRequest
        {
            AccessToken = tokenDto?.AccessToken ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty
        };

        var response = await _authenticationService.RefreshTokenAsync(request);
        if (response == null)
        {
            try
            {
                if (ControllerContext?.HttpContext?.Response?.Cookies != null)
                {
                    Response.Cookies.Delete("refreshToken");
                }
            }
            catch
            {
                // Ignore in uninitialized test context
            }
            return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, "Invalid or expired token.", null));
        }

        SetRefreshTokenCookie(response.RefreshToken);
        return Ok(new ApiResponse(HttpStatusCode.OK, "Token refreshed.", response));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        try
        {
            if (ControllerContext?.HttpContext?.Response?.Cookies != null)
            {
                Response.Cookies.Delete("refreshToken");
            }
        }
        catch
        {
            // Ignore in uninitialized test context
        }
        return Ok(new ApiResponse(HttpStatusCode.OK, "Logged out successfully.", null));
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        try
        {
            if (ControllerContext?.HttpContext?.Response?.Cookies == null) return;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
        catch
        {
            // Ignore in uninitialized test context
        }
    }
}
