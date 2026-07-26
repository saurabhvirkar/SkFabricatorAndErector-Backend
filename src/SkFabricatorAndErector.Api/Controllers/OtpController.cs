using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SkFabricatorAndErector.Api.Common;
using SkFabricatorAndErector.Api.Extensions;
using SkFabricatorAndErector.Application.Interfaces.Services;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/otp")]
[EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]
public class OtpController(IOtpService otpService) : ControllerBase
{
    private readonly IOtpService _otpService = otpService;

    [HttpPost("request")]
    [Authorize]
    public async Task<IActionResult> RequestOtpAsync([FromBody] OtpRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "ChangePasswordStepUp" : request.Purpose;
        var channel = string.IsNullOrWhiteSpace(request.Channel) ? "Email" : request.Channel;

        var otpId = await _otpService.GenerateAndSendAsync(userId, purpose, channel);
        return Ok(new ApiResponse(HttpStatusCode.OK, "Verification code sent successfully.", new { OtpId = otpId }));
    }
}

public class OtpRequestDto
{
    public string Purpose { get; set; } = "ChangePasswordStepUp";
    public string Channel { get; set; } = "Email";
}
