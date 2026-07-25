using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;
using SkFabricatorAndErector.Application.Interfaces.Persistence;
using SkFabricatorAndErector.Application.Interfaces.Services;
using SkFabricatorAndErector.Domain.Entities;

namespace SkFabricatorAndErector.Application.Features.Catalog;

public class ClientDetailsService(IClientDetailsRepository clientDetailsRepository, IPhotoService photoService) : IClientDetailsService
{
    private readonly IClientDetailsRepository _clientDetailsRepository = clientDetailsRepository;
    private readonly IPhotoService _photoService = photoService;

    public async Task<IEnumerable<ClientDetails>> GetAllClientDetailsAsync()
    {
        return await _clientDetailsRepository.GetAllAsync();
    }

    public async Task<ClientDetails?> GetClientDetailsByIdAsync(int id)
    {
        return await _clientDetailsRepository.GetByIdAsync(id);
    }

    public async Task<ClientDetails> CreateClientDetailsAsync(CreateClientDetailsRequest request)
    {
        var client = new ClientDetails
        {
            Name = request.Name,
            ClientUrl = request.ClientUrl,
            ImageUrl = string.Empty
        };

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadResult = await _photoService.AddPhotoAsync(request.ImageFile);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                client.ImageUrl = uploadResult.Url;
            }
        }

        await _clientDetailsRepository.AddAsync(client);
        return client;
    }

    public async Task<ClientDetails?> UpdateClientDetailsAsync(int id, UpdateClientDetailsRequest request)
    {
        var client = await _clientDetailsRepository.GetByIdAsync(id);
        if (client == null) return null;

        client.Name = request.Name;
        client.ClientUrl = request.ClientUrl;

        if (request.ImageFile != null && request.ImageFile.Length > 0)
        {
            var uploadResult = await _photoService.AddPhotoAsync(request.ImageFile);
            if (!string.IsNullOrEmpty(uploadResult.Url))
            {
                client.ImageUrl = uploadResult.Url;
            }
        }

        await _clientDetailsRepository.UpdateAsync(client);
        return client;
    }

    public async Task<bool> DeleteClientDetailsAsync(int id)
    {
        var client = await _clientDetailsRepository.GetByIdAsync(id);
        if (client == null) return false;

        await _clientDetailsRepository.DeleteAsync(client);
        return true;
    }
}
