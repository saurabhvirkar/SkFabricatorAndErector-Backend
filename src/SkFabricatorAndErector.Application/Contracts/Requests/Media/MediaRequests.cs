using Microsoft.AspNetCore.Http;

namespace SkFabricatorAndErector.Application.Contracts.Requests.Media;

public class AddPhotoRequest
{
    public IFormFile File { get; set; } = null!;
    public string Category { get; set; } = string.Empty;
    public bool IsAboutSlider { get; set; }
}

public class AddHomeSliderRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}
