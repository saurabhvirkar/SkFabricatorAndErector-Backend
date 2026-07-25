using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Catalog;

public class ProjectService(IProjectRepository projectRepository, IPhotoService photoService) : IProjectService
{
    private readonly IProjectRepository _projectRepository = projectRepository;
    private readonly IPhotoService _photoService = photoService;

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _projectRepository.GetAllAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Project>> GetProjectsByCategoryAsync(string category)
    {
        return await _projectRepository.GetByCategoryAsync(category);
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request)
    {
        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            Category = request.Category
        };

        if (request.File != null && request.File.Length > 0)
        {
            var uploadResult = await _photoService.AddPhotoAsync(request.File);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                project.Image = uploadResult.Url;
                project.PublicId = uploadResult.PublicId;
            }
        }

        await _projectRepository.AddAsync(project);
        return project;
    }

    public async Task<Project?> UpdateProjectAsync(int id, UpdateProjectRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return null;

        project.Title = request.Title;
        project.Description = request.Description;
        project.Category = request.Category;

        if (request.File != null && request.File.Length > 0)
        {
            if (!string.IsNullOrEmpty(project.PublicId))
            {
                await _photoService.DeletePhotoAsync(project.PublicId);
            }

            var uploadResult = await _photoService.AddPhotoAsync(request.File);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                project.Image = uploadResult.Url;
                project.PublicId = uploadResult.PublicId;
            }
        }

        await _projectRepository.UpdateAsync(project);
        return project;
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null) return false;

        if (!string.IsNullOrEmpty(project.PublicId))
        {
            await _photoService.DeletePhotoAsync(project.PublicId);
        }

        await _projectRepository.DeleteAsync(project);
        return true;
    }
}
