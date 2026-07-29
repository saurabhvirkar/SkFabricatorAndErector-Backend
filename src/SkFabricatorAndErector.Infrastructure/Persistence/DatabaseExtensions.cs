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

            var rawConnStr = configuration.GetConnectionString("DefaultConnection") ?? "";
            var isPostgres = provider == "postgres" || provider == "postgresql" || 
                             rawConnStr.StartsWith("postgres", StringComparison.OrdinalIgnoreCase) || 
                             rawConnStr.Contains("neon.tech", StringComparison.OrdinalIgnoreCase);

            if (isPostgres)
            {
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

    public static string LastInitLog = "Not initialized yet";

    public static async Task UseDatabaseInitialization(this IApplicationBuilder app, IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var logBuilder = new System.Text.StringBuilder();

        void AppendLog(string msg)
        {
            var line = $"[{DateTime.UtcNow:HH:mm:ss}] {msg}";
            logBuilder.AppendLine(line);
            LastInitLog = logBuilder.ToString();
        }

        AppendLog("Starting database initialization...");

        try
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            AppendLog($"DbContext provider: {context.Database.ProviderName}");

            try
            {
                await context.Database.MigrateAsync();
                AppendLog("EF Core Database Migrations applied successfully.");
            }
            catch (Exception mEx)
            {
                AppendLog($"MigrateAsync warning: {mEx.Message}. Trying EnsureCreatedAsync.");
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    AppendLog("EnsureCreatedAsync completed.");
                }
                catch (Exception ecEx)
                {
                    AppendLog($"EnsureCreatedAsync error: {ecEx.Message}");
                }
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
                            -- AspNetUsers table boolean columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'EmailConfirmed' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""EmailConfirmed"" DROP DEFAULT;
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""EmailConfirmed"" TYPE boolean USING (CASE WHEN ""EmailConfirmed""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""EmailConfirmed"" SET DEFAULT false;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'PhoneNumberConfirmed' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PhoneNumberConfirmed"" DROP DEFAULT;
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PhoneNumberConfirmed"" TYPE boolean USING (CASE WHEN ""PhoneNumberConfirmed""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PhoneNumberConfirmed"" SET DEFAULT false;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'TwoFactorEnabled' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""TwoFactorEnabled"" DROP DEFAULT;
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""TwoFactorEnabled"" TYPE boolean USING (CASE WHEN ""TwoFactorEnabled""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""TwoFactorEnabled"" SET DEFAULT false;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'LockoutEnabled' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""LockoutEnabled"" DROP DEFAULT;
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""LockoutEnabled"" TYPE boolean USING (CASE WHEN ""LockoutEnabled""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""LockoutEnabled"" SET DEFAULT false;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'PasswordChangeRequired' AND data_type = 'integer') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PasswordChangeRequired"" DROP DEFAULT;
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PasswordChangeRequired"" TYPE boolean USING (CASE WHEN ""PasswordChangeRequired""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PasswordChangeRequired"" SET DEFAULT false;
                            END IF;

                            -- Photos table boolean columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Photos' AND column_name = 'IsAboutSlider' AND data_type = 'integer') THEN
                                ALTER TABLE ""Photos"" ALTER COLUMN ""IsAboutSlider"" DROP DEFAULT;
                                ALTER TABLE ""Photos"" ALTER COLUMN ""IsAboutSlider"" TYPE boolean USING (CASE WHEN ""IsAboutSlider""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""Photos"" ALTER COLUMN ""IsAboutSlider"" SET DEFAULT false;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Photos' AND column_name = 'IsMain' AND data_type = 'integer') THEN
                                ALTER TABLE ""Photos"" ALTER COLUMN ""IsMain"" DROP DEFAULT;
                                ALTER TABLE ""Photos"" ALTER COLUMN ""IsMain"" TYPE boolean USING (CASE WHEN ""IsMain""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""Photos"" ALTER COLUMN ""IsMain"" SET DEFAULT false;
                            END IF;

                            -- Projects table boolean columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Projects' AND column_name = 'IsFeatured' AND data_type = 'integer') THEN
                                ALTER TABLE ""Projects"" ALTER COLUMN ""IsFeatured"" DROP DEFAULT;
                                ALTER TABLE ""Projects"" ALTER COLUMN ""IsFeatured"" TYPE boolean USING (CASE WHEN ""IsFeatured""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""Projects"" ALTER COLUMN ""IsFeatured"" SET DEFAULT false;
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'Projects' AND column_name = 'IsCompleted' AND data_type = 'integer') THEN
                                ALTER TABLE ""Projects"" ALTER COLUMN ""IsCompleted"" DROP DEFAULT;
                                ALTER TABLE ""Projects"" ALTER COLUMN ""IsCompleted"" TYPE boolean USING (CASE WHEN ""IsCompleted""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""Projects"" ALTER COLUMN ""IsCompleted"" SET DEFAULT false;
                            END IF;

                            -- OurServices table boolean columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'OurServices' AND column_name = 'IsActive' AND data_type = 'integer') THEN
                                ALTER TABLE ""OurServices"" ALTER COLUMN ""IsActive"" DROP DEFAULT;
                                ALTER TABLE ""OurServices"" ALTER COLUMN ""IsActive"" TYPE boolean USING (CASE WHEN ""IsActive""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""OurServices"" ALTER COLUMN ""IsActive"" SET DEFAULT true;
                            END IF;

                            -- TeamMembers table boolean columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'TeamMembers' AND column_name = 'IsActive' AND data_type = 'integer') THEN
                                ALTER TABLE ""TeamMembers"" ALTER COLUMN ""IsActive"" DROP DEFAULT;
                                ALTER TABLE ""TeamMembers"" ALTER COLUMN ""IsActive"" TYPE boolean USING (CASE WHEN ""IsActive""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""TeamMembers"" ALTER COLUMN ""IsActive"" SET DEFAULT true;
                            END IF;

                            -- HomeSliders table boolean columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HomeSliders' AND column_name = 'IsActive' AND data_type = 'integer') THEN
                                ALTER TABLE ""HomeSliders"" ALTER COLUMN ""IsActive"" DROP DEFAULT;
                                ALTER TABLE ""HomeSliders"" ALTER COLUMN ""IsActive"" TYPE boolean USING (CASE WHEN ""IsActive""::text = '1' THEN true ELSE false END);
                                ALTER TABLE ""HomeSliders"" ALTER COLUMN ""IsActive"" SET DEFAULT true;
                            END IF;

                            -- AspNetUsers DateTime text columns
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'RefreshTokenExpiryTime' AND data_type LIKE '%text%') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""RefreshTokenExpiryTime"" DROP DEFAULT;
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""RefreshTokenExpiryTime"" TYPE timestamp with time zone USING (CASE WHEN ""RefreshTokenExpiryTime"" IS NULL OR ""RefreshTokenExpiryTime"" = '' THEN '0001-01-01 00:00:00+00'::timestamp with time zone ELSE ""RefreshTokenExpiryTime""::timestamp with time zone END);
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""RefreshTokenExpiryTime"" SET DEFAULT '0001-01-01 00:00:00+00';
                            END IF;
                            IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'AspNetUsers' AND column_name = 'LockoutEnd' AND data_type LIKE '%text%') THEN
                                ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""LockoutEnd"" TYPE timestamp with time zone USING (CASE WHEN ""LockoutEnd"" IS NULL OR ""LockoutEnd"" = '' THEN NULL ELSE ""LockoutEnd""::timestamp with time zone END);
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
                    AppendLog("PostgreSQL schema columns and identity sequences verified successfully.");
                }
                catch (Exception sqlEx)
                {
                    AppendLog($"PostgreSQL schema alter warning: {sqlEx.Message}");
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
