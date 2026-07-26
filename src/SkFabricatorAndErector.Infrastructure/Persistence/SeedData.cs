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
                await roleManager.CreateAsync(role);
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingClaimValues = existingClaims.Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value).ToHashSet();

            foreach (var permission in permissions)
            {
                if (!existingClaimValues.Contains(permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                }
            }
        }

        // 2. One-Time Secure SuperAdmin Bootstrap Process
        var superAdminEmail = "superadmin@skfabricator.com";
        var existingSuperAdmin = await userManager.GetUsersInRoleAsync(UserRoles.SuperAdmin);

        if (!existingSuperAdmin.Any())
        {
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                var bootstrapPassword = configuration["BOOTSTRAP_ADMIN_PASSWORD"]
                                        ?? configuration["SeedUserPasswords:SuperAdmin"];

                bool generatedTempPassword = false;
                if (string.IsNullOrWhiteSpace(bootstrapPassword) || bootstrapPassword.StartsWith("REPLACE_WITH_"))
                {
                    bootstrapPassword = GenerateRandomPassword();
                    generatedTempPassword = true;
                }

                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    Role = UserRoles.SuperAdmin,
                    EmailConfirmed = true,
                    PasswordChangeRequired = true
                };

                var createResult = await userManager.CreateAsync(superAdminUser, bootstrapPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, UserRoles.SuperAdmin);
                    if (generatedTempPassword)
                    {
                        logger?.LogCritical("==========================================================");
                        logger?.LogCritical("ONE-TIME SUPERADMIN BOOTSTRAP CREATED: {Email}", superAdminEmail);
                        logger?.LogCritical("TEMPORARY BOOTSTRAP PASSWORD: {Password}", bootstrapPassword);
                        logger?.LogCritical("PLEASE LOG IN AND CHANGE THIS PASSWORD IMMEDIATELY.");
                        logger?.LogCritical("==========================================================");
                    }
                    else
                    {
                        logger?.LogInformation("Successfully bootstrapped SuperAdmin user ({Email}).", superAdminEmail);
                    }
                }
                else
                {
                    logger?.LogError("Failed to bootstrap SuperAdmin user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // 3. Admin Seed Account Initialization / Password Sync
        var adminEmail = "admin@skfabricator.com";
        var adminPassword = configuration["SeedUserPasswords:Admin"];
        if (!string.IsNullOrWhiteSpace(adminPassword) && !adminPassword.StartsWith("REPLACE_WITH_"))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, Role = UserRoles.Admin, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                }
            }
            else
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);
                if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
                {
                    await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                }
            }
        }

        // 4. Manager Seed Account Initialization / Password Sync
        var managerEmail = "manager@skfabricator.com";
        var managerPassword = configuration["SeedUserPasswords:Manager"];
        if (!string.IsNullOrWhiteSpace(managerPassword) && !managerPassword.StartsWith("REPLACE_WITH_"))
        {
            var manager = await userManager.FindByEmailAsync(managerEmail);
            if (manager == null)
            {
                manager = new ApplicationUser { UserName = managerEmail, Email = managerEmail, Role = UserRoles.Manager, EmailConfirmed = true };
                var result = await userManager.CreateAsync(manager, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, UserRoles.Manager);
                }
            }
            else
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(manager);
                await userManager.ResetPasswordAsync(manager, resetToken, managerPassword);
                if (!await userManager.IsInRoleAsync(manager, UserRoles.Manager))
                {
                    await userManager.AddToRoleAsync(manager, UserRoles.Manager);
                }
            }
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
}
