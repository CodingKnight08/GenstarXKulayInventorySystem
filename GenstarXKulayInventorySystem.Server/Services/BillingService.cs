using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;

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
    // Billings 
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


    //Purchase OrderBillings

    public async Task<List<PurchaseOrderBillingDto>> GetAllPurchaseOrderBillingAsync()
    {
        List<PurchaseOrderBilling> purchaseOrderBillings = await _context.PurchaseOrderBillings.AsNoTracking().AsSplitQuery().Include(e => e.PurchaseOrder).Where(e => !e.IsDeleted).ToListAsync();
        if (purchaseOrderBillings == null || purchaseOrderBillings.Count == 0)
        {
            return new List<PurchaseOrderBillingDto>();
        }
        List<PurchaseOrderBillingDto> purchaseOrderBillingDtos = _mapper.Map<List<PurchaseOrderBillingDto>>(purchaseOrderBillings);
        return purchaseOrderBillingDtos;
    }

    public async Task<PurchaseOrderBillingDto?> GetPurchaseOrderBillingById(int id)
    {
        var purchaseOrderBilling = await _context.PurchaseOrderBillings.AsNoTracking().Include(e => e.PurchaseOrder).ThenInclude(e => e.Supplier).FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return purchaseOrderBilling == null ? null : _mapper.Map<PurchaseOrderBillingDto>(purchaseOrderBilling);
    }

    public async Task<bool> AddPurchaseOrderBilling(PurchaseOrderBillingDto purchaseOrderBilling)
    {
        var exists = await _context.PurchaseOrderBillings
            .AsNoTracking()
            .AnyAsync(x => x.PurchaseOrderId == purchaseOrderBilling.PurchaseOrderId
                        && x.PurchaseOrderBillingDate == purchaseOrderBilling.PurchaseOrderBillingDate);

        if (exists)
            return false;

        try
        {
            var billing = _mapper.Map<PurchaseOrderBilling>(purchaseOrderBilling);
            billing.CreatedBy = GetCurrentUsername();
            billing.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            billing.ExpectedPaymentDate = CalculateExpectedPaymentDate(
                billing.PaymentTermsOption,
                billing.PurchaseOrderBillingDate,
                0);

            await _context.PurchaseOrderBillings.AddAsync(billing);

            await _context.SaveChangesAsync();

            billing.PurchaseOrderBillingNumber = $"POB-{billing.Id:D7}-{DateTime.Now.Year.ToString()}";

            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding purchase order billing");
            return false;
        }
    }





    public async Task<bool> UpdatePurchaseOrderBilling(PurchaseOrderBillingDto purchaseOrderBilling)
    {
        try
        {
            var existingBilling = await _context.PurchaseOrderBillings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == purchaseOrderBilling.Id && !x.IsDeleted);
            if (existingBilling == null)
                return false;
            var billing = _mapper.Map<PurchaseOrderBilling>(purchaseOrderBilling);
            billing.UpdatedBy = GetCurrentUsername();
            billing.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
            int customDay = purchaseOrderBilling.CustomPaymentTermsOption ?? 0;
            billing.ExpectedPaymentDate = CalculateExpectedPaymentDate(
                billing.PaymentTermsOption,
                billing.PurchaseOrderBillingDate,
                customDay);
            _context.PurchaseOrderBillings.Update(billing);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase order billing");
            return false;
        }
    }

    public async Task<bool> DeletePurchaseOrderBilling(int id)
    {
        try
        {
            var purchaseOrderBilling = await _context.PurchaseOrderBillings
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (purchaseOrderBilling == null)
                return false;
            purchaseOrderBilling.IsDeleted = true;
            purchaseOrderBilling.DeletedAt = UtilitiesHelper.GetPhilippineTime();
            _context.PurchaseOrderBillings.Update(purchaseOrderBilling);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting purchase order billing");
            return false;
        }
    }
    private DateTime CalculateExpectedPaymentDate(PaymentTermsOption terms, DateTime billingDate, int customDate = 0)
    {
        return terms switch
        {
            PaymentTermsOption.Today => billingDate,
            PaymentTermsOption.SevenDays => billingDate.AddDays(7),
            PaymentTermsOption.ThirtyDays => billingDate.AddDays(30),
            PaymentTermsOption.SixtyDays => billingDate.AddDays(60),
            PaymentTermsOption.NinetyDays => billingDate.AddDays(90),
            PaymentTermsOption.Custom =>  billingDate.AddDays(customDate), // fallback if no custom date is provided
            _ => billingDate
        };
    }


}

public interface IBillingService
{
    Task<List<BillingDto>> GetAllBillingAsync();
    Task<BillingDto?> GetBillingById(int id);
    Task<bool> AddBillingAsync(BillingDto billingDto);
    Task<bool> UpdateBillingAsync(BillingDto billingDto);
    Task<bool> DeleteBillingAsync(int id);


    Task<List<PurchaseOrderBillingDto>> GetAllPurchaseOrderBillingAsync();
    Task<PurchaseOrderBillingDto?> GetPurchaseOrderBillingById(int id);
    Task<bool> AddPurchaseOrderBilling(PurchaseOrderBillingDto purchaseOrderBilling);
    Task<bool> UpdatePurchaseOrderBilling(PurchaseOrderBillingDto purchaseOrderBilling);
    Task<bool> DeletePurchaseOrderBilling(int id);
}