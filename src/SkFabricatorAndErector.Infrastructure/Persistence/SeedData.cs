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
        var rolePermissions = GetRolePermissionMatrix();
        foreach (var (roleName, permissions) in rolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole(roleName);
                try
                {
                    await roleManager.CreateAsync(role);
                }
                catch (Exception rEx)
                {
                    logger?.LogWarning("Notice: Unable to create role '{Role}': {Message}", roleName, rEx.Message);
                }
            }

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

        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        // 2. SuperAdmin Bootstrap Account
        var superAdminEmail = "superadmin@skfabricator.com";
        var superAdminPassword = configuration["BOOTSTRAP_ADMIN_PASSWORD"]
                                 ?? configuration["SeedUserPasswords:SuperAdmin"];
        if (string.IsNullOrWhiteSpace(superAdminPassword) || superAdminPassword.StartsWith("REPLACE_WITH_"))
        {
            superAdminPassword = "SuperAdmin@123!";
        }
        await EnsureUserAsync(userManager, logger, superAdminEmail, superAdminPassword, UserRoles.SuperAdmin);

        // 3. Admin Seed Account Initialization
        var adminEmail = "admin@skfabricator.com";
        var adminPassword = configuration["SeedUserPasswords:Admin"];
        if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.StartsWith("REPLACE_WITH_"))
        {
            adminPassword = "Admin@123!";
        }
        await EnsureUserAsync(userManager, logger, adminEmail, adminPassword, UserRoles.Admin);

        // 4. Manager Seed Account Initialization
        var managerEmail = "manager@skfabricator.com";
        var managerPassword = configuration["SeedUserPasswords:Manager"];
        if (string.IsNullOrWhiteSpace(managerPassword) || managerPassword.StartsWith("REPLACE_WITH_"))
        {
            managerPassword = "Manager@123!";
        }
        await EnsureUserAsync(userManager, logger, managerEmail, managerPassword, UserRoles.Manager);
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
        UserManager<ApplicationUser> userManager,
        ILogger? logger,
        string email,
        string password,
        string role)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email) 
                       ?? await userManager.FindByNameAsync(email)
                       ?? userManager.Users.FirstOrDefault(u => u.Email != null && u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Role = role,
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    LockoutEnd = null,
                    AccessFailedCount = 0,
                    PasswordChangeRequired = false
                };

                var createRes = await userManager.CreateAsync(user, password);
                if (!createRes.Succeeded)
                {
                    logger?.LogWarning("CreateAsync failed for {Email}: {Errors}", email, string.Join(", ", createRes.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.LockoutEnd = null;
                user.AccessFailedCount = 0;
                user.Role = role;
                user.PasswordChangeRequired = false;
                await userManager.UpdateAsync(user);

                if (await userManager.HasPasswordAsync(user))
                {
                    await userManager.RemovePasswordAsync(user);
                }

                var addRes = await userManager.AddPasswordAsync(user, password);
                if (!addRes.Succeeded)
                {
                    logger?.LogWarning("AddPasswordAsync failed for {Email}: {Errors}. Attempting password reset fallback.", email, string.Join(", ", addRes.Errors.Select(e => e.Description)));
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    await userManager.ResetPasswordAsync(user, resetToken, password);
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }

            logger?.LogInformation("Successfully verified and initialized seed user: {Email}", email);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to ensure seed user {Email}", email);
        }
    }
}
