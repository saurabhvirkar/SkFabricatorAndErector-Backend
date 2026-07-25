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

        return Ok(new ApiResponse(HttpStatusCode.OK, "Login successful.", response));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest tokenDto)
    {
        var response = await _authenticationService.RefreshTokenAsync(tokenDto);
        if (response == null)
        {
            return BadRequest(new ApiResponse(HttpStatusCode.BadRequest, "Invalid or expired token.", null));
        }

        return Ok(new ApiResponse(HttpStatusCode.OK, "Token refreshed.", response));
    }
}
