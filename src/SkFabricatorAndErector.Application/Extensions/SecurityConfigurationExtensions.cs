using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace SkFabricatorAndErector.Application.Extensions;

public static class SecurityConfigurationExtensions
{
    public static void ValidateStartupSecurity(this IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)) return;

        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("REPLACE_WITH_") || jwtKey.StartsWith("DEV_ONLY_"))
        {
            throw new InvalidOperationException("CRITICAL SECURITY ERROR: Production Jwt:Key is missing or unconfigured.");
        }

        var connString = configuration.GetConnectionString("DefaultConnection") ?? configuration.GetConnectionString("SqliteConnection");
        if (string.IsNullOrWhiteSpace(connString) || connString.StartsWith("REPLACE_WITH_"))
        {
            throw new InvalidOperationException("CRITICAL SECURITY ERROR: Production Database Connection String is missing or unconfigured.");
        }

        var cloudName = configuration["CloudinarySettings:CloudName"];
        var apiKey = configuration["CloudinarySettings:ApiKey"];
        var apiSecret = configuration["CloudinarySettings:ApiSecret"];
        if (string.IsNullOrWhiteSpace(cloudName) || cloudName.StartsWith("REPLACE_WITH_") ||
            string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("REPLACE_WITH_") ||
            string.IsNullOrWhiteSpace(apiSecret) || apiSecret.StartsWith("REPLACE_WITH_"))
        {
            // Log warning for missing media storage credentials instead of crashing web api startup
            Console.WriteLine("WARNING: Cloudinary credentials are missing or unconfigured. Media upload features will be disabled until configured.");
        }
    }
}
