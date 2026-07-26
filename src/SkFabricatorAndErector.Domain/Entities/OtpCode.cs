namespace SkFabricatorAndErector.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // "PasswordReset", "ChangePasswordStepUp"
    public string DeliveryChannel { get; set; } = "Email"; // "Email", "Sms"
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
    public int AttemptCount { get; set; } = 0;
    public string? RequestedByIp { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
