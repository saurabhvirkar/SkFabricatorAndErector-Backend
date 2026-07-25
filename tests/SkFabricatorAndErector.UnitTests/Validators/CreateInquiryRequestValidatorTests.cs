using SkFabricatorAndErector.Application.Contracts.Requests.Inquiries;
using SkFabricatorAndErector.Application.Validators;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Validators;

public class CreateInquiryRequestValidatorTests
{
    private readonly CreateInquiryRequestValidator _validator = new();

    [Fact]
    public void Should_HaveError_WhenNameIsEmpty()
    {
        var request = new CreateInquiryRequest { Name = "", Email = "test@example.com", Message = "Hello" };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateInquiryRequest.Name));
    }

    [Fact]
    public void Should_HaveError_WhenEmailIsInvalid()
    {
        var request = new CreateInquiryRequest { Name = "John", Email = "not-an-email", Message = "Hello" };
        var result = _validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateInquiryRequest.Email));
    }

    [Fact]
    public void Should_BeValid_WhenAllRequiredFieldsAreCorrect()
    {
        var request = new CreateInquiryRequest { Name = "John Doe", Email = "john@example.com", Message = "Valid message" };
        var result = _validator.Validate(request);
        Assert.True(result.IsValid);
    }
}
