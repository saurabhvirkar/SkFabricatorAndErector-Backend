using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Persistence;

public interface IProjectRepository : IGenericRepository<Project>
{
    Task<IEnumerable<Project>> GetByCategoryAsync(string category);
}

public interface IOurServiceRepository : IGenericRepository<OurService>
{
}

public interface ITeamMemberRepository : IGenericRepository<TeamMember>
{
}

public interface IClientDetailsRepository : IGenericRepository<ClientDetails>
{
}
