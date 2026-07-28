using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider")?.ToLowerInvariant() ?? "sqlite";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            if (provider == "postgres" || provider == "postgresql")
            {
                var rawConnStr = configuration.GetConnectionString("DefaultConnection") ?? "";
                var formattedConnStr = ConvertPostgresConnectionString(rawConnStr);
                options.UseNpgsql(formattedConnStr);
            }
            else
            {
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection") ?? "Data Source=skfabricator.db");
            }
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        return services;
    }

    public static async Task UseDatabaseInitialization(this IApplicationBuilder app, IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        try
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("EF Core Database Migrations applied successfully.");
            }
            catch (Exception mEx)
            {
                logger.LogWarning(mEx, "MigrateAsync fallback. Executing EnsureCreatedAsync.");
                await context.Database.EnsureCreatedAsync();
            }

            if (context.Database.IsNpgsql())
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(@"
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN IF NOT EXISTS ""Role"" text NULL;
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN IF NOT EXISTS ""RefreshToken"" text NULL;
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN IF NOT EXISTS ""RefreshTokenExpiryTime"" timestamp with time zone NOT NULL DEFAULT '0001-01-01 00:00:00+00';
                        ALTER TABLE ""AspNetUsers"" ADD COLUMN IF NOT EXISTS ""PasswordChangeRequired"" boolean NOT NULL DEFAULT false;
                    ");
                    logger.LogInformation("PostgreSQL AspNetUsers schema columns verified successfully.");
                }
                catch (Exception sqlEx)
                {
                    logger.LogWarning(sqlEx, "Notice: Unable to execute column alter verification on AspNetUsers.");
                }
            }

            await SeedData.InitializeAsync(serviceProvider, configuration);
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the DB.");
        }
    }

    private static string ConvertPostgresConnectionString(string connString)
    {
        if (string.IsNullOrWhiteSpace(connString)) return connString;
        if (connString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(connString);
                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
                var database = uri.AbsolutePath.TrimStart('/');
                var host = uri.Host;
                var port = uri.Port > 0 ? uri.Port : 5432;

                return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
            }
            catch
            {
                return connString;
            }
        }
        return connString;
    }
}
