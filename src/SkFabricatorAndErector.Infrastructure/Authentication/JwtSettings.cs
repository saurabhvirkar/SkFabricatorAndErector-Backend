namespace SkFabricatorAndErector.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpireDays { get; set; } = 1;
    public int RefreshTokenExpireDays { get; set; } = 7;
}
