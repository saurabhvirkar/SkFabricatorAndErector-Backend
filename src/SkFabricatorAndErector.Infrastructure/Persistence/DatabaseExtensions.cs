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

                        DO $$
                        BEGIN
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'EmailConfirmed' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""EmailConfirmed"" TYPE boolean USING (""EmailConfirmed""::int::boolean);
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'PhoneNumberConfirmed' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PhoneNumberConfirmed"" TYPE boolean USING (""PhoneNumberConfirmed""::int::boolean);
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'TwoFactorEnabled' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""TwoFactorEnabled"" TYPE boolean USING (""TwoFactorEnabled""::int::boolean);
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'LockoutEnabled' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""LockoutEnabled"" TYPE boolean USING (""LockoutEnabled""::int::boolean);
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'PasswordChangeRequired' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PasswordChangeRequired"" TYPE boolean USING (""PasswordChangeRequired""::int::boolean);
                            END IF;

                            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetRoleClaims') THEN
                                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetRoleClaims' AND column_name = 'Id' AND is_identity = 'YES') THEN
                                    ALTER TABLE ""AspNetRoleClaims"" ALTER COLUMN ""Id"" ADD GENERATED BY DEFAULT AS IDENTITY;
                                END IF;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUserClaims') THEN
                                IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUserClaims' AND column_name = 'Id' AND is_identity = 'YES') THEN
                                    ALTER TABLE ""AspNetUserClaims"" ALTER COLUMN ""Id"" ADD GENERATED BY DEFAULT AS IDENTITY;
                                END IF;
                            END IF;
                        END $$;
                    ");
                    logger.LogInformation("PostgreSQL schema columns and identity sequences verified successfully.");
                }
                catch (Exception sqlEx)
                {
                    logger.LogWarning(sqlEx, "Notice: Unable to execute schema alter verification on PostgreSQL.");
                }
            }
            else if (context.Database.IsSqlite())
            {
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD COLUMN Role TEXT NULL;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD COLUMN RefreshToken TEXT NULL;"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD COLUMN RefreshTokenExpiryTime TEXT NOT NULL DEFAULT '0001-01-01 00:00:00';"); } catch { }
                try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE AspNetUsers ADD COLUMN PasswordChangeRequired INTEGER NOT NULL DEFAULT 0;"); } catch { }
                logger.LogInformation("SQLite schema columns verified successfully.");
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
