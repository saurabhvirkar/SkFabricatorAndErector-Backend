using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(SeedData));

        var adminPassword = configuration["SeedUserPasswords:Admin"];
        var managerPassword = configuration["SeedUserPasswords:Manager"];

        string[] roles = new[] { "Admin", "Manager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin
        var adminEmail = "admin@skfabricator.com";
        if (!string.IsNullOrWhiteSpace(adminPassword) && !adminPassword.StartsWith("REPLACE_WITH_"))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, Role = "Admin" };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    logger?.LogInformation("Successfully created seed Admin user ({AdminEmail}).", adminEmail);
                }
                else
                {
                    logger?.LogError("Failed to create seed Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Ensure password is synchronized with configuration
                var token = await userManager.GeneratePasswordResetTokenAsync(admin);
                await userManager.ResetPasswordAsync(admin, token, adminPassword);
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
                logger?.LogInformation("Successfully updated seed Admin user ({AdminEmail}) password and roles.", adminEmail);
            }
        }
        else
        {
            logger?.LogWarning("Skipping seed for Admin user ({AdminEmail}): 'SeedUserPasswords:Admin' is missing or unconfigured.", adminEmail);
        }

        // Manager
        var managerEmail = "manager@skfabricator.com";
        if (!string.IsNullOrWhiteSpace(managerPassword) && !managerPassword.StartsWith("REPLACE_WITH_"))
        {
            var manager = await userManager.FindByEmailAsync(managerEmail);
            if (manager == null)
            {
                manager = new ApplicationUser { UserName = managerEmail, Email = managerEmail, Role = "Manager" };
                var result = await userManager.CreateAsync(manager, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Manager");
                    logger?.LogInformation("Successfully created seed Manager user ({ManagerEmail}).", managerEmail);
                }
                else
                {
                    logger?.LogError("Failed to create seed Manager user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Ensure password is synchronized with configuration
                var token = await userManager.GeneratePasswordResetTokenAsync(manager);
                await userManager.ResetPasswordAsync(manager, token, managerPassword);
                if (!await userManager.IsInRoleAsync(manager, "Manager"))
                {
                    await userManager.AddToRoleAsync(manager, "Manager");
                }
                logger?.LogInformation("Successfully updated seed Manager user ({ManagerEmail}) password and roles.", managerEmail);
            }
        }
        else
        {
            logger?.LogWarning("Skipping seed for Manager user ({ManagerEmail}): 'SeedUserPasswords:Manager' is missing or unconfigured.", managerEmail);
        }
    }
}
