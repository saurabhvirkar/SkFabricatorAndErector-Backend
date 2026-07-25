using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Infrastructure.Persistence.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context, ILogger<GenericRepository<Project>>? logger = null)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Project>> GetByCategoryAsync(string category)
    {
        return await _context.Projects.Where(p => p.Category == category).ToListAsync();
    }
}

public class OurServiceRepository : GenericRepository<OurService>, IOurServiceRepository
{
    public OurServiceRepository(ApplicationDbContext context, ILogger<GenericRepository<OurService>>? logger = null)
        : base(context, logger)
    {
    }
}

public class TeamMemberRepository : GenericRepository<TeamMember>, ITeamMemberRepository
{
    public TeamMemberRepository(ApplicationDbContext context, ILogger<GenericRepository<TeamMember>>? logger = null)
        : base(context, logger)
    {
    }
}

public class ClientDetailsRepository : GenericRepository<ClientDetails>, IClientDetailsRepository
{
    public ClientDetailsRepository(ApplicationDbContext context, ILogger<GenericRepository<ClientDetails>>? logger = null)
        : base(context, logger)
    {
    }
}
