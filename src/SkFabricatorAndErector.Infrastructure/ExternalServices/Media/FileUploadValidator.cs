using Microsoft.AspNetCore.Http;

namespace SkFabricatorAndErector.Infrastructure.ExternalServices.Media;

public static class FileUploadValidator
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/pjpeg", "image/png", "image/webp"
    };

    public static string? ValidateImageFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return "No file provided or file is empty.";
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            return "File size exceeds the maximum limit of 5 MB.";
        }

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            return $"Invalid file extension '{ext}'. Only .jpg, .jpeg, .png, and .webp files are allowed.";
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return $"Invalid Content-Type '{file.ContentType}'. Allowed types: image/jpeg, image/png, image/webp.";
        }

        if (!IsValidMagicBytes(file))
        {
            return "File content does not match a valid image signature (JPEG, PNG, or WebP).";
        }

        return null;
    }

    private static bool IsValidMagicBytes(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var header = new byte[12];
            int read = stream.Read(header, 0, header.Length);
            if (read < 12) return false;

            // JPEG magic bytes: 0xFF, 0xD8, 0xFF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return true;
            }

            // PNG magic bytes: 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            {
                return true;
            }

            // WebP magic bytes: RIFF (bytes 0..3) ... WEBP (bytes 8..11)
            if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
