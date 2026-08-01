using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SkFabricatorAndErector.Api.Controllers;

[ApiController]
[Route("api/company-profile")]
public class CompanyProfilePdfController(IWebHostEnvironment env) : ControllerBase
{
    private readonly IWebHostEnvironment _env = env;

    private IEnumerable<string> GetAllPossiblePdfPaths()
    {
        var list = new List<string>
        {
            Path.Combine(_env.ContentRootPath, "Resources", "sk-company-profile.pdf"),
            Path.Combine(AppContext.BaseDirectory, "Resources", "sk-company-profile.pdf"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "sk-company-profile.pdf"),
            Path.Combine(_env.ContentRootPath, "sk-company-profile.pdf")
        };
        return list.Distinct();
    }

    private string GetPdfPath()
    {
        var paths = GetAllPossiblePdfPaths();
        return paths.FirstOrDefault(System.IO.File.Exists) ?? paths.First();
    }

    [HttpGet("download-pdf")]
    [HttpGet("pdf")]
    [AllowAnonymous]
    public IActionResult DownloadPdf()
    {
        var finalPath = GetPdfPath();

        if (!System.IO.File.Exists(finalPath))
        {
            return NotFound(new { message = "Company profile PDF file is not available on server." });
        }

        var fileBytes = System.IO.File.ReadAllBytes(finalPath);
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return File(fileBytes, "application/pdf", "SK-Fabricator-Company-Profile.pdf");
    }

    [HttpGet("pdf-info")]
    [AllowAnonymous]
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

        byte[] fileBytes;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        // Save to all possible target paths so all lookups reflect the new file instantly
        foreach (var targetPath in GetAllPossiblePdfPaths())
        {
            try
            {
                var dir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                await System.IO.File.WriteAllBytesAsync(targetPath, fileBytes);
            }
            catch
            {
                // Ignore path write errors if directory is inaccessible
            }
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
        foreach (var targetPath in GetAllPossiblePdfPaths())
        {
            if (System.IO.File.Exists(targetPath))
            {
                try
                {
                    System.IO.File.Delete(targetPath);
                }
                catch
                {
                    // Ignore deletion failures for individual paths
                }
            }
        }

        return Ok(new { message = "Company profile PDF removed from server." });
    }
}
