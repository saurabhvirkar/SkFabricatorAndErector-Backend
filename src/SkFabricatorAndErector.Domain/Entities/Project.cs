using System.ComponentModel.DataAnnotations;

namespace SkFabricatorAndErector.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    [Required]
    public string? Title { get; set; }

    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Category { get; set; }
    public string? PublicId { get; set; }

    public string CategoryLabel { get; set; } = string.Empty;
    public string? Client { get; set; }
    public string PhotoPlaceholder { get; set; } = string.Empty;
}
