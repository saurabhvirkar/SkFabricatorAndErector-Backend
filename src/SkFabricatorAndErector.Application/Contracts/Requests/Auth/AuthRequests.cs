namespace SkFabricatorAndErector.Application.Contracts.Requests.Auth;

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class RefreshTokenRequest
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
