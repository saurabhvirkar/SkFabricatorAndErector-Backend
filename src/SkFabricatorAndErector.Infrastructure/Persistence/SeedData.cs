using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SeedData));

        // 1. Seed Roles & Role Claims (Permission Matrix)
        try
        {
            var rolePermissions = GetRolePermissionMatrix();
            foreach (var (roleName, permissions) in rolePermissions)
            {
                IdentityRole? role = null;
                try
                {
                    role = await roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        role = new IdentityRole(roleName);
                        await roleManager.CreateAsync(role);
                    }
                }
                catch (Exception rEx)
                {
                    logger?.LogWarning("Notice: Unable to create role '{Role}': {Message}", roleName, rEx.Message);
                }

                if (role != null)
                {
                    try
                    {
                        var existingClaims = await roleManager.GetClaimsAsync(role);
                        var existingClaimValues = existingClaims.Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value).ToHashSet();

                        foreach (var permission in permissions)
                        {
                            if (!existingClaimValues.Contains(permission))
                            {
                                try
                                {
                                    await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                                }
                                catch (Exception claimEx)
                                {
                                    logger?.LogWarning("Notice: Unable to seed claim '{Permission}' for role '{Role}': {Message}", permission, roleName, claimEx.Message);
                                }
                            }
                        }
                    }
                    catch (Exception claimsGetEx)
                    {
                        logger?.LogWarning("Notice: Unable to query claims for role '{Role}': {Message}", roleName, claimsGetEx.Message);
                    }
                }
            }
        }
        catch (Exception rolesEx)
        {
            logger?.LogWarning(rolesEx, "Notice: Role seeding encountered an exception.");
        }

        // 2. SuperAdmin Bootstrap Account
        try
        {
            var superAdminEmail = "superadmin@skfabricator.com";
            var superAdminPassword = configuration["BOOTSTRAP_ADMIN_PASSWORD"]
                                     ?? configuration["SeedUserPasswords:SuperAdmin"];
            if (string.IsNullOrWhiteSpace(superAdminPassword) || superAdminPassword.StartsWith("REPLACE_WITH_"))
            {
                superAdminPassword = "SuperAdmin@123!";
            }
            await EnsureUserAsync(serviceProvider, userManager, logger, superAdminEmail, superAdminPassword, UserRoles.SuperAdmin);
        }
        catch (Exception saEx)
        {
            logger?.LogError(saEx, "Failed to seed SuperAdmin account");
        }

        // 3. Admin Seed Account Initialization
        try
        {
            var adminEmail = "admin@skfabricator.com";
            var adminPassword = configuration["SeedUserPasswords:Admin"];
            if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.StartsWith("REPLACE_WITH_"))
            {
                adminPassword = "Admin@123!";
            }
            await EnsureUserAsync(serviceProvider, userManager, logger, adminEmail, adminPassword, UserRoles.Admin);
        }
        catch (Exception aEx)
        {
            logger?.LogError(aEx, "Failed to seed Admin account");
        }

        // 4. Manager Seed Account Initialization
        try
        {
            var managerEmail = "manager@skfabricator.com";
            var managerPassword = configuration["SeedUserPasswords:Manager"];
            if (string.IsNullOrWhiteSpace(managerPassword) || managerPassword.StartsWith("REPLACE_WITH_"))
            {
                managerPassword = "Manager@123!";
            }
            await EnsureUserAsync(serviceProvider, userManager, logger, managerEmail, managerPassword, UserRoles.Manager);
        }
        catch (Exception mEx)
        {
            logger?.LogError(mEx, "Failed to seed Manager account");
        }
    }

    private static Dictionary<string, List<string>> GetRolePermissionMatrix()
    {
        var allPermissions = new List<string>
        {
            Permissions.Projects.Read, Permissions.Projects.Create, Permissions.Projects.Update, Permissions.Projects.Delete,
            Permissions.Services.Read, Permissions.Services.Create, Permissions.Services.Update, Permissions.Services.Delete,
            Permissions.Team.Read, Permissions.Team.Create, Permissions.Team.Update, Permissions.Team.Delete,
            Permissions.Gallery.Read, Permissions.Gallery.Create, Permissions.Gallery.Delete,
            Permissions.Clients.Read, Permissions.Clients.Create, Permissions.Clients.Update, Permissions.Clients.Delete,
            Permissions.HomeSlider.Read, Permissions.HomeSlider.Create, Permissions.HomeSlider.Delete,
            Permissions.Inquiries.Read, Permissions.Inquiries.Delete,
            Permissions.Users.Read, Permissions.Users.Create, Permissions.Users.Update, Permissions.Users.Disable,
            Permissions.Roles.Read, Permissions.Roles.Assign,
            Permissions.Audit.Read,
            Permissions.Security.Manage
        };

        var adminPermissions = new List<string>
        {
            Permissions.Projects.Read, Permissions.Projects.Create, Permissions.Projects.Update, Permissions.Projects.Delete,
            Permissions.Services.Read, Permissions.Services.Create, Permissions.Services.Update, Permissions.Services.Delete,
            Permissions.Team.Read, Permissions.Team.Create, Permissions.Team.Update, Permissions.Team.Delete,
            Permissions.Gallery.Read, Permissions.Gallery.Create, Permissions.Gallery.Delete,
            Permissions.Clients.Read, Permissions.Clients.Create, Permissions.Clients.Update, Permissions.Clients.Delete,
            Permissions.HomeSlider.Read, Permissions.HomeSlider.Create, Permissions.HomeSlider.Delete,
            Permissions.Inquiries.Read, Permissions.Inquiries.Delete,
            Permissions.Users.Read, Permissions.Users.Create, Permissions.Users.Update, Permissions.Users.Disable,
            Permissions.Audit.Read
        };

        var managerPermissions = new List<string>
        {
            Permissions.Projects.Read, Permissions.Projects.Create, Permissions.Projects.Update,
            Permissions.Services.Read, Permissions.Services.Create, Permissions.Services.Update,
            Permissions.Team.Read, Permissions.Team.Create, Permissions.Team.Update,
            Permissions.Gallery.Read, Permissions.Gallery.Create,
            Permissions.Clients.Read, Permissions.Clients.Create, Permissions.Clients.Update,
            Permissions.HomeSlider.Read, Permissions.HomeSlider.Create,
            Permissions.Inquiries.Read
        };

        var employeePermissions = new List<string>
        {
            Permissions.Projects.Read, Permissions.Projects.Create, Permissions.Projects.Update,
            Permissions.Services.Read, Permissions.Services.Create, Permissions.Services.Update,
            Permissions.Team.Read, Permissions.Team.Create, Permissions.Team.Update,
            Permissions.Gallery.Read, Permissions.Gallery.Create,
            Permissions.Clients.Read, Permissions.Clients.Create, Permissions.Clients.Update,
            Permissions.HomeSlider.Read, Permissions.HomeSlider.Create
        };

        return new Dictionary<string, List<string>>
        {
            { UserRoles.SuperAdmin, allPermissions },
            { UserRoles.Admin, adminPermissions },
            { UserRoles.Manager, managerPermissions },
            { UserRoles.Employee, employeePermissions },
            { UserRoles.Customer, new List<string>() }
        };
    }

    private static string GenerateRandomPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%^&*";

        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var chars = new[]
        {
            upper[bytes[0] % upper.Length],
            lower[bytes[1] % lower.Length],
            digits[bytes[2] % digits.Length],
            special[bytes[3] % special.Length]
        }.ToList();

        for (int i = 4; i < 16; i++)
        {
            string source = (i % 4) switch
            {
                0 => upper,
                1 => lower,
                2 => digits,
                _ => special
            };
            chars.Add(source[bytes[i] % source.Length]);
        }

        return new string(chars.OrderBy(_ => bytes[0]).ToArray());
    }

    private static async Task EnsureUserAsync(
        IServiceProvider serviceProvider,
        UserManager<ApplicationUser> userManager,
        ILogger? logger,
        string email,
        string password,
        string role)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var passwordHasher = new PasswordHasher<ApplicationUser>();

        try
        {
            var user = await userManager.FindByEmailAsync(email) 
                       ?? await userManager.FindByNameAsync(email)
                       ?? await userManager.FindByEmailAsync(normalizedEmail)
                       ?? userManager.Users.FirstOrDefault(u => u.Email != null && (u.Email.ToLower() == email.ToLower() || u.UserName.ToLower() == email.ToLower()));

            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    Email = email,
                    NormalizedUserName = normalizedEmail,
                    NormalizedEmail = normalizedEmail,
                    Role = role,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    LockoutEnd = null,
                    AccessFailedCount = 0,
                    PasswordChangeRequired = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                user.PasswordHash = passwordHasher.HashPassword(user, password);
                var createRes = await userManager.CreateAsync(user);
                if (!createRes.Succeeded)
                {
                    logger?.LogWarning("UserManager CreateAsync failed for {Email}: {Errors}. Trying DbContext direct insert.", email, string.Join(", ", createRes.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                user.UserName = email;
                user.Email = email;
                user.NormalizedEmail = normalizedEmail;
                user.NormalizedUserName = normalizedEmail;
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
                user.AccessFailedCount = 0;
                user.Role = role;
                user.PasswordChangeRequired = false;
                user.SecurityStamp = Guid.NewGuid().ToString();

                user.PasswordHash = passwordHasher.HashPassword(user, password);
                var updateRes = await userManager.UpdateAsync(user);
                if (!updateRes.Succeeded)
                {
                    logger?.LogWarning("UserManager UpdateAsync failed for {Email}: {Errors}. Trying DbContext direct update.", email, string.Join(", ", updateRes.Errors.Select(e => e.Description)));
                }
            }

            try
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            catch (Exception rEx)
            {
                logger?.LogWarning(rEx, "Unable to assign role {Role} to {Email}", role, email);
            }
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "UserManager encountered an error for {Email}. Falling back to direct ApplicationDbContext persistence.", email);
        }

        // Direct ApplicationDbContext Failsafe to guarantee user exists in PostgreSQL DB
        try
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var dbUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email || u.NormalizedEmail == normalizedEmail);
            if (dbUser == null)
            {
                dbUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = email,
                    Email = email,
                    NormalizedUserName = normalizedEmail,
                    NormalizedEmail = normalizedEmail,
                    Role = role,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    LockoutEnd = null,
                    AccessFailedCount = 0,
                    PasswordChangeRequired = false,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                dbUser.PasswordHash = passwordHasher.HashPassword(dbUser, password);
                dbContext.Users.Add(dbUser);
                await dbContext.SaveChangesAsync();
                logger?.LogInformation("Direct ApplicationDbContext created seed user: {Email}", email);
            }
            else
            {
                dbUser.UserName = email;
                dbUser.Email = email;
                dbUser.NormalizedEmail = normalizedEmail;
                dbUser.NormalizedUserName = normalizedEmail;
                dbUser.EmailConfirmed = true;
                dbUser.LockoutEnabled = false;
                dbUser.LockoutEnd = null;
                dbUser.AccessFailedCount = 0;
                dbUser.Role = role;
                dbUser.PasswordChangeRequired = false;
                dbUser.SecurityStamp = Guid.NewGuid().ToString();
                dbUser.PasswordHash = passwordHasher.HashPassword(dbUser, password);
                dbContext.Users.Update(dbUser);
                await dbContext.SaveChangesAsync();
                logger?.LogInformation("Direct ApplicationDbContext updated seed user: {Email}", email);
            }
        }
        catch (Exception dbEx)
        {
            logger?.LogError(dbEx, "Direct ApplicationDbContext failsafe error for {Email}", email);
        }
    }
}
