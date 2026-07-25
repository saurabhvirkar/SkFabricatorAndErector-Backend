using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SkFabricatorAndErector.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth-fixed";
    public const string GeneralPolicy = "general-fixed";

    /// <summary>
    /// Adds rate limiting policies:
    /// - "auth-fixed": tight limit for auth endpoints (brute force protection)
    /// - "general-fixed": relaxed limit for all other API calls
    /// </summary>
    public static IServiceCollection AddRateLimitingPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Strict: 10 requests per minute per IP — for /api/account/* endpoints
            options.AddFixedWindowLimiter(AuthPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });

            // Relaxed: 300 requests per minute per IP — for general endpoints
            options.AddFixedWindowLimiter(GeneralPolicy, limiterOptions =>
            {
                limiterOptions.PermitLimit = 300;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }
}
