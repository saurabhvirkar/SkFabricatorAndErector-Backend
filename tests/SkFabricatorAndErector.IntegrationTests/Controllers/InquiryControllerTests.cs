using Microsoft.AspNetCore.Mvc;
using Moq;
using SkFabricatorAndErector.Api.Controllers;
using SkFabricatorAndErector.Application.Contracts.Requests.Inquiries;
using SkFabricatorAndErector.Application.Contracts.Responses.Inquiries;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;
using Xunit;

namespace SkFabricatorAndErector.IntegrationTests.Controllers;

public class InquiryControllerTests
{
    private readonly Mock<IInquiryService> _serviceMock = new();
    private readonly InquiryController _controller;

    public InquiryControllerTests()
    {
        _controller = new InquiryController(_serviceMock.Object);
    }

    [Fact]
    public async Task SubmitInquiryAsync_ShouldReturnCreatedAtRoute_WithInquiryResponse()
    {
        // Arrange
        var request = new CreateInquiryRequest
        {
            Name = "Alice Smith",
            Email = "alice@example.com",
            Message = "Looking for fabrication quote."
        };

        _serviceMock.Setup(s => s.CreateInquiryAsync(It.IsAny<Inquiry>()))
                    .ReturnsAsync((Inquiry i) => { i.Id = 10; return i; });

        // Act
        var actionResult = await _controller.SubmitInquiryAsync(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtRouteResult>(actionResult);
        Assert.Equal("GetInquiryByIdAsync", createdResult.RouteName);
        var response = Assert.IsType<InquiryResponse>(createdResult.Value);
        Assert.Equal(10, response.Id);
        Assert.Equal("Alice Smith", response.Name);
    }

    [Fact]
    public async Task GetInquiryByIdAsync_ShouldReturnNotFound_WhenInquiryDoesNotExist()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetInquiryByIdAsync(999)).ReturnsAsync((Inquiry?)null);

        // Act
        var result = await _controller.GetInquiryByIdAsync(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
