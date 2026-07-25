namespace SkFabricatorAndErector.Application.Contracts.Responses.Inquiries;

public class InquiryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string? Category { get; set; }
    public string? PreferredContact { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
