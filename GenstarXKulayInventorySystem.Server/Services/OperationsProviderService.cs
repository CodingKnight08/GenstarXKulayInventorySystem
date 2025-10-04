using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Services;

public class OperationsProviderService:IOperationsProviderService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<OperationsProviderService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBillingService _billingService;
    public OperationsProviderService(InventoryDbContext context, ILogger<OperationsProviderService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IBillingService billingService)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _billingService = billingService;
    }
    private string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
    }
    public async Task<List<OperationsProviderDto>> GetAllOperationsProvider(BranchOption branch)
    {
        List<OperationsProvider> operationsProviders = await _context.OperationsProviders.AsNoTracking().Where(op => !op.IsDeleted && op.Branch == branch).ToListAsync();
        if (operationsProviders == null || operationsProviders.Count == 0)
        {
            return new List<OperationsProviderDto>();
        }
        return _mapper.Map<List<OperationsProviderDto>>(operationsProviders);
    }

    public async Task<OperationsProviderDto?> GetOperationsProviderById(int id)
    {
        OperationsProvider? operationsProvider = await _context.OperationsProviders.AsNoTracking().FirstOrDefaultAsync(op => !op.IsDeleted && op.Id == id);
        if (operationsProvider == null)
        {
            return null;
        }
        return _mapper.Map<OperationsProviderDto>(operationsProvider);
    }

    public async Task<int> AddOperationsProvider(OperationsProviderDto operationsProvider)
    {
        try
        {
            var existingProvider = await _context.OperationsProviders.AsNoTracking().FirstOrDefaultAsync(op => (op.ProviderName == operationsProvider.ProviderName || op.TINNumber == operationsProvider.TINNumber) && op.Branch == operationsProvider.Branch);
            if (existingProvider != null)
                return 0;
            var operationProvider = _mapper.Map<OperationsProvider>(operationsProvider);
            operationProvider.CreatedBy = GetCurrentUsername();
            operationProvider.CreatedAt = DateTime.UtcNow;
            _ = await _context.OperationsProviders.AddAsync(operationProvider);
            _ = await _context.SaveChangesAsync();
            
            return operationProvider.Id;

        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error adding provider");
            return 0;
        }
    }


    public async Task<bool> UpdateOperationsProvider(OperationsProviderDto operationsProvider)
    {
        try
        {
            var existingProvider = await _context.OperationsProviders.FirstOrDefaultAsync(op => !op.IsDeleted && op.Id == operationsProvider.Id);
            if (existingProvider == null)
                return false;
            var duplicateProvider = await _context.OperationsProviders.AsNoTracking().FirstOrDefaultAsync(op => (op.ProviderName == operationsProvider.ProviderName || op.TINNumber == operationsProvider.TINNumber) && op.Id != operationsProvider.Id && op.Branch == operationsProvider.Branch);
            if (duplicateProvider != null)
                return false;
            existingProvider.ProviderName = operationsProvider.ProviderName;
            existingProvider.TINNumber = operationsProvider.TINNumber;
            existingProvider.Address = operationsProvider.Address;
            existingProvider.Branch = operationsProvider.Branch;
            existingProvider.UpdatedBy = GetCurrentUsername();
            existingProvider.UpdatedAt = DateTime.UtcNow;
            _context.OperationsProviders.Update(existingProvider);
            _ = await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating provider");
            return false;
        }
    }

    public async Task<bool> DeleteOperationsProvider(int id)
    {
        try
        {
            var existingProvider = await _context.OperationsProviders.FirstOrDefaultAsync(op => !op.IsDeleted && op.Id == id);
            if (existingProvider == null)
                return false;
            existingProvider.IsDeleted = true;
            existingProvider.DeletedAt = DateTime.UtcNow;
            existingProvider.UpdatedBy = GetCurrentUsername();
            existingProvider.UpdatedAt = DateTime.UtcNow;
            _context.OperationsProviders.Update(existingProvider);
            _ = await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider");
            return false;
        }
    }


}
public interface IOperationsProviderService
{
    Task<List<OperationsProviderDto>> GetAllOperationsProvider(BranchOption branch);
    Task<OperationsProviderDto?> GetOperationsProviderById(int id);
    Task<int> AddOperationsProvider(OperationsProviderDto operationsProvider);
    Task<bool> UpdateOperationsProvider(OperationsProviderDto operationsProvider);
    Task<bool> DeleteOperationsProvider(int id);
}