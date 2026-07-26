namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IOtpService
{
    Task<Guid> GenerateAndSendAsync(string userId, string purpose, string channel = "Email");
    Task<bool> VerifyAsync(string userId, string purpose, string submittedCode);
}
