using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Infrastructure.Authentication;
using SkFabricatorAndErector.Infrastructure.ExternalServices.Email;
using SkFabricatorAndErector.Infrastructure.ExternalServices.Media;
using SkFabricatorAndErector.Infrastructure.Logging;
using SkFabricatorAndErector.Infrastructure.Persistence;
using SkFabricatorAndErector.Infrastructure.Persistence.Repositories;

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
        var key = Encoding.UTF8.GetBytes(jwtSettings.Key);

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
                // Prevent the default 5-minute clock skew — tokens must not be accepted after exact expiry
                ClockSkew = TimeSpan.Zero
            };
        });

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

        services.AddScoped<IPhotoService, CloudinaryPhotoService>();
        services.AddTransient<IEmailService, MailKitEmailService>();

        return services;
    }
}
