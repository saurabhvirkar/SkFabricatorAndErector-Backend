using Microsoft.AspNetCore.Authorization;
using SkFabricatorAndErector.Domain.Constants;

namespace SkFabricatorAndErector.Infrastructure.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Check for permission claim or exact permission claim match
        var hasPermission = context.User.Claims.Any(c =>
            (c.Type == Permissions.ClaimType || c.Type == "permission") &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
