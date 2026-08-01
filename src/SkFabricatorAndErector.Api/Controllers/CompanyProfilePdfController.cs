using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/company-profile")]
public class CompanyProfilePdfController(IWebHostEnvironment env) : ControllerBase
{
    private readonly IWebHostEnvironment _env = env;

    private string GetPdfPath()
    {
        var paths = new[]
        {
            Path.Combine(_env.ContentRootPath, "Resources", "sk-company-profile.pdf"),
            Path.Combine(AppContext.BaseDirectory, "Resources", "sk-company-profile.pdf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "sk-company-profile.pdf"),
            Path.Combine(_env.ContentRootPath, "sk-company-profile.pdf")
        };

        return paths.FirstOrDefault(System.IO.File.Exists) ?? paths[0];
    }

    [HttpGet("download-pdf")]
    [HttpGet("pdf")]
    public IActionResult DownloadPdf()
    {
        var finalPath = GetPdfPath();

        if (!System.IO.File.Exists(finalPath))
        {
            return NotFound(new { message = "Company profile PDF file is not available on server." });
        }

        var fileBytes = System.IO.File.ReadAllBytes(finalPath);
        return File(fileBytes, "application/pdf", "SK-Fabricator-Company-Profile.pdf");
    }

    [HttpGet("pdf-info")]
    public IActionResult GetPdfInfo()
    {
        var pdfPath = GetPdfPath();
        if (!System.IO.File.Exists(pdfPath))
        {
            return Ok(new
            {
                exists = false,
                fileName = "SK-Fabricator-Company-Profile.pdf",
                fileSizeBytes = 0,
                fileSizeMb = 0.0,
                updatedAtUtc = (DateTime?)null
            });
        }

        var info = new FileInfo(pdfPath);
        return Ok(new
        {
            exists = true,
            fileName = info.Name,
            fileSizeBytes = info.Length,
            fileSizeMb = Math.Round((double)info.Length / (1024 * 1024), 2),
            updatedAtUtc = info.LastWriteTimeUtc
        });
    }

    [HttpPost("upload-pdf")]
    [Authorize]
    public async Task<IActionResult> UploadPdf(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Please select a valid PDF file to upload." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            return BadRequest(new { message = "Invalid file type. Only PDF (.pdf) documents are allowed." });
        }

        var resourcesDir = Path.Combine(_env.ContentRootPath, "Resources");
        if (!Directory.Exists(resourcesDir))
        {
            Directory.CreateDirectory(resourcesDir);
        }

        var targetPath = Path.Combine(resourcesDir, "sk-company-profile.pdf");

        using (var stream = new FileStream(targetPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Ok(new
        {
            message = "Company Profile PDF uploaded successfully.",
            fileName = "sk-company-profile.pdf",
            fileSizeMb = Math.Round((double)file.Length / (1024 * 1024), 2),
            updatedAtUtc = DateTime.UtcNow
        });
    }

    [HttpDelete("delete-pdf")]
    [Authorize]
    public IActionResult DeletePdf()
    {
        var pdfPath = GetPdfPath();
        if (System.IO.File.Exists(pdfPath))
        {
            try
            {
                System.IO.File.Delete(pdfPath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error deleting PDF: {ex.Message}" });
            }
        }

        return Ok(new { message = "Company profile PDF removed from server." });
    }
}
