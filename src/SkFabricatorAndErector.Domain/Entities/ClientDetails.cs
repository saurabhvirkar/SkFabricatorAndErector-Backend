using System.ComponentModel.DataAnnotations;

namespace SkFabricatorAndErector.Domain.Entities;

public class ClientDetails
{
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? ImageUrl { get; set; }
    public string? ClientUrl { get; set; }
}
