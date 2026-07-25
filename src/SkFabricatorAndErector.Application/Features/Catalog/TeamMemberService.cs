using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Catalog;

public class TeamMemberService(ITeamMemberRepository teamMemberRepository, IPhotoService photoService) : ITeamMemberService
{
    private readonly ITeamMemberRepository _teamMemberRepository = teamMemberRepository;
    private readonly IPhotoService _photoService = photoService;

    public async Task<IEnumerable<TeamMember>> GetAllTeamMembersAsync()
    {
        return await _teamMemberRepository.GetAllAsync();
    }

    public async Task<TeamMember?> GetTeamMemberByIdAsync(int id)
    {
        return await _teamMemberRepository.GetByIdAsync(id);
    }

    public async Task<TeamMember> CreateTeamMemberAsync(CreateTeamMemberRequest request)
    {
        var member = new TeamMember
        {
            Name = request.Name,
            Role = request.Role,
            Email = request.Email,
            LinkedInUrl = request.LinkedInUrl,
            Details = request.Details,
            ImageUrl = string.Empty
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadResult = await _photoService.AddPhotoAsync(request.ImageFile);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                member.ImageUrl = uploadResult.Url;
                member.PublicId = uploadResult.PublicId;
            }
        }

        await _teamMemberRepository.AddAsync(member);
        return member;
    }

    public async Task<TeamMember?> UpdateTeamMemberAsync(int id, UpdateTeamMemberRequest request)
    {
        var member = await _teamMemberRepository.GetByIdAsync(id);
        if (member == null) return null;

        member.Name = request.Name;
        member.Role = request.Role;
        member.Email = request.Email;
        member.LinkedInUrl = request.LinkedInUrl;
        member.Details = request.Details;

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(member.PublicId))
            {
                await _photoService.DeletePhotoAsync(member.PublicId);
            }

            var uploadResult = await _photoService.AddPhotoAsync(request.ImageFile);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                member.ImageUrl = uploadResult.Url;
                member.PublicId = uploadResult.PublicId;
            }
        }

        await _teamMemberRepository.UpdateAsync(member);
        return member;
    }

    public async Task<bool> DeleteTeamMemberAsync(int id)
    {
        var member = await _teamMemberRepository.GetByIdAsync(id);
        if (member == null) return false;

        if (!string.IsNullOrEmpty(member.PublicId))
        {
            await _photoService.DeletePhotoAsync(member.PublicId);
        }

        await _teamMemberRepository.DeleteAsync(member);
        return true;
    }
}
