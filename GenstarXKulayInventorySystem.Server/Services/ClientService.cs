using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Services;

public class ClientService: IClientService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ClientService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClientService(InventoryDbContext context, IMapper mapper, ILogger<ClientService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
    }

    public async Task<List<ClientDto>> GetAllClientsAsync(BranchOption branch)
    {
        List<Client> clients = await _context.Clients
            .AsNoTracking()
            .AsSplitQuery()
            .Where(c => !c.IsDeleted && c.Branch == branch)
            .OrderBy(c => c.ClientName).ToListAsync();

        if (clients == null || clients.Count == 0)
        {
            return new List<ClientDto>();
        } 
        List<ClientDto> clientDtos = _mapper.Map<List<ClientDto>>(clients);
        return clientDtos;
    }

    public async Task<ClientDto?> GetClientById(int id)
    {
        var client = await _context.Clients.Include(c => c.DailySales).FirstOrDefaultAsync(c => !c.IsDeleted && c.Id == id);
        return client == null ? null : _mapper.Map<ClientDto>(client);
    }


    public async Task<int?> AddClientAsync(ClientDto clientDto)
    {
        try
        {
            // Check if client already exists
            var existingClient = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientName == clientDto.ClientName);

            if (existingClient != null)
            {
                return null; // or existingClient.Id if you want to return the existing Id
            }

            var client = _mapper.Map<Client>(clientDto);
            client.CreatedBy = GetCurrentUsername();
            client.CreatedAt = UtilitiesHelper.GetPhilippineTime();

            await _context.Clients.AddAsync(client);
            await _context.SaveChangesAsync();

            return client.Id; // EF sets this after SaveChanges
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating client: {ClientName}", clientDto.ClientName);
            return null;
        }
    }

    public async Task<bool> UpdateClientAsync(ClientDto clientDto)
    {
        try
        {
            var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientDto.Id && !c.IsDeleted);
            if (existingClient == null)
                return false;

            existingClient.UpdatedBy = GetCurrentUsername();
            existingClient.UpdatedAt = UtilitiesHelper.GetPhilippineTime();

            _mapper.Map(clientDto, existingClient);
            await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error in updating Client");
            return false;
        }
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return false;
        client.IsDeleted = true;
        client.DeletedAt = UtilitiesHelper.GetPhilippineTime();
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
        return true;
    }

}

public interface IClientService
{
    Task<List<ClientDto>> GetAllClientsAsync(BranchOption branch);
    Task<ClientDto?> GetClientById(int id);
    Task<int?> AddClientAsync(ClientDto clientDto);
    Task<bool> UpdateClientAsync(ClientDto clientDto);
    Task<bool> DeleteClientAsync(int id);
}
