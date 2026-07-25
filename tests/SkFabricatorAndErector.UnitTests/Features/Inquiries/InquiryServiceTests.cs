using Microsoft.Extensions.Logging;
using Moq;
using SkFabricatorAndErector.Application.Features.Inquiries;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.UnitTests.Features.Inquiries;

public class InquiryServiceTests
{
    private readonly Mock<IInquiryRepository> _repositoryMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<InquiryService>> _loggerMock = new();
    private readonly InquiryService _sut;

    public InquiryServiceTests()
    {
        _sut = new InquiryService(_repositoryMock.Object, _emailServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateInquiryAsync_ShouldAddInquiry_AndSendNotificationEmail()
    {
        // Arrange
        var inquiry = new Inquiry
        {
            Name = "John Doe",
            Email = "john@example.com",
            Message = "Need a structural steel quote."
        };

        // Act
        var result = await _sut.CreateInquiryAsync(inquiry);

        // Assert
        Assert.NotNull(result);
        _repositoryMock.Verify(r => r.AddAsync(inquiry), Times.Once);
        _emailServiceMock.Verify(e => e.SendInquiryNotificationEmailAsync(inquiry), Times.Once);
    }

    [Fact]
    public async Task DeleteInquiryAsync_ShouldReturnFalse_WhenInquiryNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Inquiry?)null);

        // Act
        var result = await _sut.DeleteInquiryAsync(99);

        // Assert
        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Inquiry>()), Times.Never);
    }

    [Fact]
    public async Task DeleteInquiryAsync_ShouldReturnTrue_WhenInquiryExists()
    {
        // Arrange
        var inquiry = new Inquiry { Id = 1, Name = "Test" };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inquiry);

        // Act
        var result = await _sut.DeleteInquiryAsync(1);

        // Assert
        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(inquiry), Times.Once);
    }
}
