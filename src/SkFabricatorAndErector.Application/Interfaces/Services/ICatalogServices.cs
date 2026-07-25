using Microsoft.AspNetCore.Http;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Interfaces.Services;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(int id);
    Task<IEnumerable<Project>> GetProjectsByCategoryAsync(string category);
    Task<Project> CreateProjectAsync(CreateProjectRequest request);
    Task<Project?> UpdateProjectAsync(int id, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(int id);
}

public interface IOurServiceService
{
    Task<IEnumerable<OurService>> GetAllServicesAsync();
    Task<OurService?> GetServiceByIdAsync(int id);
    Task<OurService> CreateServiceAsync(CreateOurServiceRequest request);
    Task<OurService?> UpdateServiceAsync(int id, UpdateOurServiceRequest request);
    Task<bool> DeleteServiceAsync(int id);
}

public interface ITeamMemberService
{
    Task<IEnumerable<TeamMember>> GetAllTeamMembersAsync();
    Task<TeamMember?> GetTeamMemberByIdAsync(int id);
    Task<TeamMember> CreateTeamMemberAsync(CreateTeamMemberRequest request);
    Task<TeamMember?> UpdateTeamMemberAsync(int id, UpdateTeamMemberRequest request);
    Task<bool> DeleteTeamMemberAsync(int id);
}

public interface IClientDetailsService
{
    Task<IEnumerable<ClientDetails>> GetAllClientDetailsAsync();
    Task<ClientDetails?> GetClientDetailsByIdAsync(int id);
    Task<ClientDetails> CreateClientDetailsAsync(CreateClientDetailsRequest request);
    Task<ClientDetails?> UpdateClientDetailsAsync(int id, UpdateClientDetailsRequest request);
    Task<bool> DeleteClientDetailsAsync(int id);
}
