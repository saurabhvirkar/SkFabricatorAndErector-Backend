using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Catalog;

public class OurServiceService(IOurServiceRepository serviceRepository, IPhotoService photoService) : IOurServiceService
{
    private readonly IOurServiceRepository _serviceRepository = serviceRepository;
    private readonly IPhotoService _photoService = photoService;

    public async Task<IEnumerable<OurService>> GetAllServicesAsync()
    {
        return await _serviceRepository.GetAllAsync();
    }

    public async Task<OurService?> GetServiceByIdAsync(int id)
    {
        return await _serviceRepository.GetByIdAsync(id);
    }

    public async Task<OurService> CreateServiceAsync(CreateOurServiceRequest request)
    {
        var service = new OurService
        {
            Name = request.Name,
            Summary = request.Summary,
            Description = request.Description
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadResult = await _photoService.AddPhotoAsync(request.ImageFile);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                service.ImageUrl = uploadResult.Url;
            }
        }

        await _serviceRepository.AddAsync(service);
        return service;
    }

    public async Task<OurService?> UpdateServiceAsync(int id, UpdateOurServiceRequest request)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null) return null;

        service.Name = request.Name;
        service.Summary = request.Summary;
        service.Description = request.Description;

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadResult = await _photoService.AddPhotoAsync(request.ImageFile);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                service.ImageUrl = uploadResult.Url;
            }
        }

        await _serviceRepository.UpdateAsync(service);
        return service;
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null) return false;

        await _serviceRepository.DeleteAsync(service);
        return true;
    }
}
