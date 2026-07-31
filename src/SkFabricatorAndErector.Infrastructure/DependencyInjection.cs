using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Infrastructure.Authentication;
using SkFabricatorAndErector.Infrastructure.Authorization;
using SkFabricatorAndErector.Infrastructure.ExternalServices.Email;
using SkFabricatorAndErector.Infrastructure.ExternalServices.Media;
using SkFabricatorAndErector.Infrastructure.Logging;
using SkFabricatorAndErector.Infrastructure.Persistence;
using SkFabricatorAndErector.Infrastructure.Persistence.Repositories;
using SkFabricatorAndErector.Infrastructure.Services;

namespace SkFabricatorAndErector.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));

        services.AddPersistence(configuration, environment);

        // JWT Authentication Configuration
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        var rawKey = string.IsNullOrWhiteSpace(jwtSettings.Key) ? "DEFAULT_SECRET_KEY_MIN_32_BYTES_PADDING_SECURE" : jwtSettings.Key;
        var key = Encoding.UTF8.GetBytes(rawKey);
        if (key.Length < 32)
        {
            Array.Resize(ref key, 32);
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });

        // Dynamic Permission Policy Provider & Handler
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ISecurityAuditLogger, SecurityAuditLogger>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IInquiryRepository, InquiryRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IHomeSliderRepository, HomeSliderRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IOurServiceRepository, OurServiceRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<IClientDetailsRepository, ClientDetailsRepository>();
        services.AddScoped<IPageImageSlotRepository, PageImageSlotRepository>();

        services.AddScoped<IPhotoService, CloudinaryPhotoService>();
        services.AddTransient<IEmailService, MailKitEmailService>();
        services.AddScoped<IOtpService, OtpService>();

        // Polly v8 Full Resilience Pipeline (Wait & Retry, Exponential Backoff, Circuit Breaker, Timeout, Bulkhead Isolation, Fault Data)
        services.AddHttpClient("ResilientExternalClient")
            .AddStandardResilienceHandler(options =>
            {
                // 1. Wait and Retry + Exponential Backoff + Jitter
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.Retry.OnRetry = args =>
                {
                    Console.WriteLine($"[Polly:WaitAndRetry] Attempt {args.AttemptNumber} failed. Retrying after {args.RetryDelay.TotalMilliseconds}ms... (Fault: {args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()})");
                    return ValueTask.CompletedTask;
                };

                // 2. Circuit Breaker & BrokenCircuitException
                options.CircuitBreaker.FailureRatio = 0.5;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
                options.CircuitBreaker.MinimumThroughput = 5;
                options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                options.CircuitBreaker.OnOpened = args =>
                {
                    Console.WriteLine($"[Polly:CircuitBreaker] Circuit TRIPPED to OPEN due to elevated failure ratio!");
                    return ValueTask.CompletedTask;
                };

                // 3. Timeout & TimeoutRejectedException
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}
