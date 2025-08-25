using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<ClientDto>> GetAllClientsAsync()
    {
        List<Client> clients = await _context.Clients
            .AsNoTracking()
            .AsSplitQuery()
            .Where(c => !c.IsDeleted).ToListAsync();

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


    public async Task<bool> AddClientAsync(ClientDto clientDto)
    {
        try
        {
            var existingClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.ClientName == clientDto.ClientName);
            if (existingClient != null) { 
                return false;
            }

            var client = _mapper.Map<Client>(clientDto);
            client.CreatedBy = GetCurrentUsername();
            client.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            _ = await _context.Clients.AddAsync(client);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error creating client: {ClientName}", clientDto.ClientName);
            return false;
        }
    }
}

public interface IClientService
{

}
