namespace SkFabricatorAndErector.Application.Contracts.Responses.Auth;

public class AuthenticationResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool PasswordChangeRequired { get; set; } = false;
    public List<string> Permissions { get; set; } = new();
}
