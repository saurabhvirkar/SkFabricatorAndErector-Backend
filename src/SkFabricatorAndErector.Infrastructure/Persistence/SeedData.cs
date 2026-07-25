using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, Role = "Admin" };
            await userManager.CreateAsync(admin, adminPassword ?? "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Manager
        var managerEmail = "manager@skfabricator.com";
        var manager = await userManager.FindByEmailAsync(managerEmail);
        if (manager == null)
        {
            manager = new ApplicationUser { UserName = managerEmail, Email = managerEmail, Role = "Manager" };
            await userManager.CreateAsync(manager, managerPassword ?? "Manager@123");
            await userManager.AddToRoleAsync(manager, "Manager");
        }
    }
}
