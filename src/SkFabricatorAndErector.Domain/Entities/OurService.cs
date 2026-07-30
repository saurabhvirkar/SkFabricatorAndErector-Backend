using System.ComponentModel.DataAnnotations;

namespace SkFabricatorAndErector.Domain.Entities;

public class OurService
{
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? CloudinaryPublicId { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Teaser { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public string? BulletTitle { get; set; }
    public string BulletsJson { get; set; } = "[]";
    public string PhotoPlaceholder { get; set; } = string.Empty;
    public bool Featured { get; set; }
    public int SortOrder { get; set; }
}
