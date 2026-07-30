namespace SkFabricatorAndErector.Application.Contracts.Responses.Catalog;

public class ProjectResponse
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string? Category { get; set; }
    public string? PublicId { get; set; }
    public string? CategoryLabel { get; set; }
    public string? Client { get; set; }
    public string? PhotoPlaceholder { get; set; }
}

public class OurServiceResponse
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Slug { get; set; }
    public string? Subtitle { get; set; }
    public string? Teaser { get; set; }
    public string? IconName { get; set; }
    public string? BulletTitle { get; set; }
    public string? BulletsJson { get; set; }
    public string? PhotoPlaceholder { get; set; }
    public bool Featured { get; set; }
    public int SortOrder { get; set; }
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
    public string? Tagline { get; set; }
    public string? Category { get; set; }
}
