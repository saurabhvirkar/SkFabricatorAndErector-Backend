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
            if (provider == "postgres")
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
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
            if (context.Database.IsNpgsql())
            {
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully for PostgreSQL.");
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully for SQLite.");
            }

            await SeedData.InitializeAsync(serviceProvider, configuration);
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the DB.");
        }
    }
}
