using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Inquiries;

public class InquiryService(IInquiryRepository inquiryRepository, IEmailService emailService, ILogger<InquiryService> logger) : IInquiryService
{
    private readonly IInquiryRepository _inquiryRepository = inquiryRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<InquiryService> _logger = logger;

    public async Task<Inquiry> CreateInquiryAsync(Inquiry inquiry, IFormFile? file = null)
    {
        if (inquiry.SubmittedAt == default)
        {
            inquiry.SubmittedAt = DateTime.UtcNow;
        }

        await _inquiryRepository.AddAsync(inquiry);
        await _inquiryRepository.SaveChangesAsync();

        // Fire email notification in background task so user HTTP response is never blocked by SMTP timeouts
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendInquiryNotificationEmailAsync(inquiry, file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send inquiry notification email for inquiry ID {InquiryId}.", inquiry.Id);
            }
        });

        return inquiry;
    }

    public async Task<IEnumerable<Inquiry>> GetAllInquiriesAsync()
    {
        return await _inquiryRepository.GetAllAsync();
    }

    public async Task<Inquiry?> GetInquiryByIdAsync(int id)
    {
        return await _inquiryRepository.GetByIdAsync(id);
    }

    public async Task<bool> DeleteInquiryAsync(int id)
    {
        var inquiryToDelete = await _inquiryRepository.GetByIdAsync(id);
        if (inquiryToDelete == null)
        {
            return false; // Inquiry not found
        }

        await _inquiryRepository.DeleteAsync(inquiryToDelete);
        await _inquiryRepository.SaveChangesAsync();
        return true; // Deletion successful
    }
}
