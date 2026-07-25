using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IInquiryService
{
    Task<Inquiry> CreateInquiryAsync(Inquiry inquiry);
    Task<IEnumerable<Inquiry>> GetAllInquiriesAsync();
    Task<Inquiry?> GetInquiryByIdAsync(int id);
    Task<bool> DeleteInquiryAsync(int id);
}
