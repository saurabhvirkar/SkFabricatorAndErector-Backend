using Microsoft.AspNetCore.Identity;

namespace SkFabricatorAndErector.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? Role { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public bool PasswordChangeRequired { get; set; } = false;
}
