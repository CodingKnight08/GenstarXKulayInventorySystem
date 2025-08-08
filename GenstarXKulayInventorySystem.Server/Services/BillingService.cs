using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GenstarXKulayInventorySystem.Server.Services;

public class BillingService:IBillingService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<BillingService> _logger;
    public BillingService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<BillingService> logger)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    private string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
    }

    public async Task<List<BillingDto>> GetAllBillingAsync()
    {
        List<Billing> billings = await _context.Billings
            .AsNoTracking()
            .AsSplitQuery()
            
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.DateOfBilling).ToListAsync();

        if (billings == null || billings.Count == 0)
        {
            return new List<BillingDto>();
        }

        List<BillingDto> billingDtos = _mapper.Map<List<BillingDto>>(billings);
        return billingDtos;
    }

  

    public async Task<List<BillingDto>> GetAllNotPurchaseOrderBillings()
    {
        var billings = await _context.Billings
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Where(e => !e.IsDeleted)
                        .OrderByDescending(e => e.DateOfBilling)
                        .ToListAsync();

        if (billings == null || billings.Count == 0)
        {
            return new List<BillingDto>();
        }
        List<BillingDto> billingDtos = _mapper.Map<List<BillingDto>>(billings);
        return billingDtos;
    }

    public async Task<BillingDto?> GetBillingById(int id)
    {
        var billing = await _context.Billings.AsNoTracking()
                             .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        return billing == null ? null : _mapper.Map<BillingDto>(billing);

    }

    public async Task<BillingDto?> GetOperationalBillingById(int id)
    {
        var billing = await _context.Billings.AsNoTracking()
                             .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return billing == null ? null : _mapper.Map<BillingDto>(billing);
    }

    public async Task<bool> AddBillingAsync(BillingDto billingDto)
    {
        try
        {
            var existingBilling = await _context.Billings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BillingNumber == billingDto.BillingNumber);
            if (existingBilling != null)
                return false;
            var billing = _mapper.Map<Billing>(billingDto);
            if (billing.IsPaid)
            {
                billing.DatePaid = UtilitiesHelper.GetPhilippineTime();
            }
            billing.CreatedBy = GetCurrentUsername();
            billing.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            await _context.Billings.AddAsync(billing);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding billing");
            return false;
        }
    }

    public async Task<bool> UpdateBillingAsync(BillingDto billingDto)
    {
        try
        {
            var existingBilling = await _context.Billings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == billingDto.Id && !x.IsDeleted);
            if (existingBilling == null)
                return false;
            var billing = _mapper.Map<Billing>(billingDto);
            if (billingDto.IsPaid)
            {
                billing.DatePaid = UtilitiesHelper.GetPhilippineTime();
            }
            billing.UpdatedBy = GetCurrentUsername();
            billing.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
            _context.Billings.Update(billing);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating billing");
            return false;
        }
    }

    public async Task<bool> DeleteBillingAsync(int id)
    {
        try
        {
            var billing = await _context.Billings
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (billing == null)
                return false;
            billing.IsDeleted = true;
            billing.DeletedAt = UtilitiesHelper.GetPhilippineTime();
            _context.Billings.Update(billing);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting billing");
            return false;
        }
    }
}

public interface IBillingService
{
    Task<List<BillingDto>> GetAllBillingAsync();
    Task<List<BillingDto>> GetAllNotPurchaseOrderBillings();
    Task<BillingDto?> GetBillingById(int id);
    Task<BillingDto?> GetOperationalBillingById(int id);
    Task<bool> AddBillingAsync(BillingDto billingDto);
    Task<bool> UpdateBillingAsync(BillingDto billingDto);
    Task<bool> DeleteBillingAsync(int id);
}