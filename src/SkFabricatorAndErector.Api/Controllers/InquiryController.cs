using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Constants;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/inquiry")]
[Route("api/inquiries")]
public class InquiryController(IInquiryService inquiryService) : ControllerBase
{
    private readonly IInquiryService _inquiryService = inquiryService;

    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    public async Task<IActionResult> CreateInquiry([FromForm] Inquiry inquiry, IFormFile? file)
    {
        var result = await _inquiryService.CreateInquiryAsync(inquiry, file);
        return CreatedAtAction(nameof(GetInquiryById), new { id = result.Id }, result);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Inquiries.Read)]
    public async Task<IActionResult> GetInquiries()
    {
        var inquiries = await _inquiryService.GetAllInquiriesAsync();
        return Ok(inquiries);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = Permissions.Inquiries.Read)]
    public async Task<IActionResult> GetInquiryById(int id)
    {
        var inquiry = await _inquiryService.GetInquiryByIdAsync(id);
        if (inquiry == null) return NotFound();
        return Ok(inquiry);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Inquiries.Delete)]
    public async Task<IActionResult> DeleteInquiry(int id)
    {
        var success = await _inquiryService.DeleteInquiryAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
