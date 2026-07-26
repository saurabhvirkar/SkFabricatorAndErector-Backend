using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Contracts.Requests.Inquiries;
using SkFabricatorAndErector.Application.Contracts.Responses.Inquiries;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/inquiry")]
public class InquiryController(IInquiryService inquiryService) : ControllerBase
{
    private readonly IInquiryService _inquiryService = inquiryService;

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitInquiryAsync([FromBody] CreateInquiryRequest request)
    {
        var inquiryEntity = new Inquiry
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Subject = request.Subject,
            Category = request.Category,
            PreferredContact = request.PreferredContact,
            Message = request.Message,
            SubmittedAt = DateTime.UtcNow
        };

        var createdInquiry = await _inquiryService.CreateInquiryAsync(inquiryEntity);
        var response = MapToResponse(createdInquiry);

        return CreatedAtRoute("GetInquiryByIdAsync", new { id = response.Id }, response);
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> GetInquiriesAsync()
    {
        var inquiries = await _inquiryService.GetAllInquiriesAsync();
        var responseList = inquiries.Select(MapToResponse);
        return Ok(responseList);
    }

    [HttpGet("{id}", Name = "GetInquiryByIdAsync")]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> GetInquiryByIdAsync(int id)
    {
        var inquiry = await _inquiryService.GetInquiryByIdAsync(id);

        if (inquiry == null)
        {
            return NotFound();
        }

        return Ok(MapToResponse(inquiry));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.AdminOrManager)]
    public async Task<IActionResult> DeleteInquiryAsync(int id)
    {
        var success = await _inquiryService.DeleteInquiryAsync(id);

        if (!success)
        {
            return NotFound($"Inquiry with ID {id} not found.");
        }

        return NoContent();
    }

    private static InquiryResponse MapToResponse(Inquiry entity)
    {
        return new InquiryResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone,
            Subject = entity.Subject,
            Category = entity.Category,
            PreferredContact = entity.PreferredContact,
            Message = entity.Message,
            SubmittedAt = entity.SubmittedAt
        };
    }
}
