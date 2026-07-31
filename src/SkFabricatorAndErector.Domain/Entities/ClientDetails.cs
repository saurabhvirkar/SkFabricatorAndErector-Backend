using System.ComponentModel.DataAnnotations;

namespace SkFabricatorAndErector.Domain.Entities;

public class ClientDetails
{
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public string? ImageUrl { get; set; } = string.Empty;
    public string? ClientUrl { get; set; }

    public string? Tagline { get; set; }
    public string Category { get; set; } = string.Empty;
}
