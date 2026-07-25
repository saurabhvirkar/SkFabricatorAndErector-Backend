namespace SkFabricatorAndErector.Application.Contracts.Responses.Media;

public class PhotoResponse
{
    public int Id { get; set; }
    public string? Url { get; set; }
    public bool IsMain { get; set; }
    public string? PublicId { get; set; }
    public bool IsAboutSlider { get; set; }
    public string? Category { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

public class HomeSliderResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? PublicId { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}
