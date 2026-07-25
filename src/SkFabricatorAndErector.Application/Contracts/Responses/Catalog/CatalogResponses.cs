namespace SkFabricatorAndErector.Application.Contracts.Responses.Catalog;

public class ProjectResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Category { get; set; }
    public string? PublicId { get; set; }
}

public class OurServiceResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class TeamMemberResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? ImageUrl { get; set; }
    public string? Email { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Details { get; set; }
    public string? PublicId { get; set; }
}

public class ClientDetailsResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? ClientUrl { get; set; }
}
