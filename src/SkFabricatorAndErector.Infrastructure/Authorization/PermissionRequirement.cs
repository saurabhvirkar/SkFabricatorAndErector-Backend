using Microsoft.AspNetCore.Authorization;

namespace SkFabricatorAndErector.Infrastructure.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
