using System.ComponentModel.DataAnnotations;

namespace SkFabricatorAndErector.Domain.Entities;

public class PageImageSlot
{
    public int Id { get; set; }

    [Required]
    public string SlotKey { get; set; } = string.Empty;

    [Required]
    public string PageName { get; set; } = string.Empty;

    [Required]
    public string SectionName { get; set; } = string.Empty;

    [Required]
    public string Label { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string? CloudinaryPublicId { get; set; }
    public string? AltText { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
