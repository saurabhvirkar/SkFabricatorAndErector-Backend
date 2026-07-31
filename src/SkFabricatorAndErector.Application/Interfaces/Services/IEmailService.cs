using Microsoft.AspNetCore.Http;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendInquiryNotificationEmailAsync(Inquiry inquiry, IFormFile? file = null);
    Task SendOtpCodeAsync(string toEmail, string code, string purpose);
}
