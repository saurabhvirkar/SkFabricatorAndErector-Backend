using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendInquiryNotificationEmailAsync(Inquiry inquiry);
}
