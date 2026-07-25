using Microsoft.AspNetCore.Http;

namespace SkFabricatorAndErector.Application.Contracts.Requests.Catalog;

public class CreateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public IFormFile? File { get; set; }
}

public class UpdateProjectRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public IFormFile? File { get; set; }
}

public class CreateOurServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public IFormFile? ImageFile { get; set; }
}

public class UpdateOurServiceRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public IFormFile? ImageFile { get; set; }
}

public class CreateTeamMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Details { get; set; }
    public IFormFile? ImageFile { get; set; }
}

public class UpdateTeamMemberRequest
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Details { get; set; }
    public IFormFile? ImageFile { get; set; }
}

public class CreateClientDetailsRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ClientUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
}

public class UpdateClientDetailsRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ClientUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
}
