using SkFabricatorAndErector.Application.Contracts.Requests.Auth;
using SkFabricatorAndErector.Application.Contracts.Responses.Auth;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResponse?> LoginAsync(LoginRequest request);
    Task<AuthenticationResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ChangePasswordResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
