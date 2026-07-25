namespace SkFabricatorAndErector.Application.Interfaces.Services;

public class ImageUploadResult
{
    public string? Url { get; set; }
    public string? PublicId { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Error { get; set; }
}
