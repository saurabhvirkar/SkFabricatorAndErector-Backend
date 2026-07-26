using System.ComponentModel.DataAnnotations;

namespace SkFabricatorAndErector.Application.Contracts.Requests.Auth;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public string? OtpCode { get; set; }
}

public record ChangePasswordResult(bool Succeeded, IEnumerable<string> Errors)
{
    public static ChangePasswordResult Success() => new(true, Array.Empty<string>());
    public static ChangePasswordResult Failed(params string[] errors) => new(false, errors);
    public static ChangePasswordResult Failed(IEnumerable<string> errors) => new(false, errors);
}
