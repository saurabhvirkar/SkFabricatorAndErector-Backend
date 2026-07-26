namespace SkFabricatorAndErector.Domain.Entities;

public class ApiClient
{
    public int Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecretHash { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string AllowedScopes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool IsActive => RevokedAt == null;
}
