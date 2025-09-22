using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;


namespace GenstarXKulayInventorySystem.Server.Services;

public class SalesService:ISalesService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SalesService> _logger;
    private readonly ISaleItemService _saleItemService;

    public SalesService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<SalesService> logger, ISaleItemService saleItemService)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _saleItemService = saleItemService;
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }

    public async Task<List<DailySaleDto>> GetAllDailySalesAsync()
    {
        List<DailySale> dailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted && e.DateOfSales.Date == DateTime.UtcNow.Date)
            .OrderByDescending(e => e.DateOfSales)
            .ToListAsync();
        if(dailySales == null || dailySales.Count == 0)
        {
            return new List<DailySaleDto>();
        }

        List<DailySaleDto> dailySaleDtos = _mapper.Map<List<DailySaleDto>>(dailySales);
        return dailySaleDtos;
    }

    public async Task<List<DailySaleDto>> GetAllDailySalesByDaySetAsync(DateTime date)
    {
        var chosenDateLocal = date.Date;

        // assume the chosenDate is local, so convert to UTC
        var startOfDay = chosenDateLocal.ToUniversalTime();
        var endOfDay = chosenDateLocal.AddDays(1).ToUniversalTime();

        var dailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted && e.DateOfSales >= startOfDay && e.DateOfSales < endOfDay)
            .OrderByDescending(e => e.DateOfSales)
            .ToListAsync();

        return _mapper.Map<List<DailySaleDto>>(dailySales);
    }


    public async Task<List<DailySaleDto>> GetAllDailySaleByDaysAsync(DateRangeOption range)
    {
        DateTime startDate = range switch
        {
            DateRangeOption.OneWeek => DateTime.UtcNow.AddDays(-7),
            DateRangeOption.OneMonth => DateTime.UtcNow.AddMonths(-1),
            DateRangeOption.TwoMonths => DateTime.UtcNow.AddMonths(-2),
            DateRangeOption.ThreeMonths => DateTime.UtcNow.AddMonths(-3),
            DateRangeOption.OneYear => DateTime.UtcNow.AddYears(-1),
            _ => DateTime.UtcNow 
        };

        var dailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted && e.DateOfSales >= startDate)
            .OrderByDescending(e => e.DateOfSales)
            .ToListAsync();

        if (dailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(dailySales);
    }

    public async Task<List<DailySaleDto>> GetAllDailySalesPaidAsync(DateTime date, BranchOption branch)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var paidDailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.Branch == branch
                      && ds.UpdatedAt == null
                      && ds.PaymentType != null
                      && ds.DateOfSales >= start
                      && ds.DateOfSales < end)
            .ToListAsync();

        if (paidDailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(paidDailySales);
    }

    public async Task<decimal> GetAllDailyInvoiceAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        decimal invoices = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.SalesOption == PurchaseRecieptOption.BIR
                      && ds.DateOfSales >= start
                      && ds.DateOfSales < end)
            .SumAsync(ds => (decimal?)ds.TotalAmount) ?? 0;


        return invoices;
    }

    public async Task<decimal> GetAllDailyNonVoiceAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        decimal nonVoices = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.SalesOption == PurchaseRecieptOption.NonBIR
                      && ds.DateOfSales >= start
                      && ds.DateOfSales < end)
            .SumAsync(ds => (decimal?)ds.TotalAmount) ?? 0;


        return nonVoices;
    }

    public async Task<List<DailySaleDto>> GetAllDailySalesUnpaidAsync(DateTime date, BranchOption branch)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var unpaidDailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.Branch == branch
                      && ds.UpdatedAt == null
                      && ds.PaymentType == null
                      && ds.DateOfSales >= start
                      && ds.DateOfSales < end)
            .ToListAsync();

        if (unpaidDailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(unpaidDailySales);
    }

    public async Task<List<DailySaleDto>> GetAllCollectionAsync(DateTime date, BranchOption branch)
    {
        var collectedSales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.Branch == branch
                      && ds.UpdatedAt.HasValue
                     )
            .ToListAsync();

        if(collectedSales == null)
        {
            return new List<DailySaleDto>();
        }
        return _mapper.Map<List<DailySaleDto>>(collectedSales);
    }


    public async Task<DailySaleDto?> GetDailySaleByIdAsync(int id)
    {
        var dailySale = await _context.DailySales.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return dailySale == null ? null : _mapper.Map<DailySaleDto>(dailySale);
    }

    public async Task<bool> AddAsync(DailySaleDto saleDto)
    {
        try
        {
            var existingSale = await _context.DailySales.AsNoTracking().FirstOrDefaultAsync(e => e.Branch == saleDto.Branch && e.Id == saleDto.Id);
            if (existingSale != null) {
                return false;
            }
            var sale = _mapper.Map<DailySale>(saleDto);
            sale.DateOfSales = DateTime.UtcNow;
            sale.CreatedAt = DateTime.UtcNow;
            sale.CreatedBy = GetCurrentUsername();
            sale.TotalAmount = saleDto.SaleItems.Sum(x => (x.ItemPrice) * (x.Quantity));
            sale.ExpectedPaymentDate = CalculateExpectedPaymentDate(
                saleDto.PaymentTermsOption ?? PaymentTermsOption.Today,
                DateTime.UtcNow,
                saleDto.CustomPaymentTermsOption ?? 0);
            _ = await _context.DailySales.AddAsync(sale);
            _ = await _context.SaveChangesAsync();
            sale.SalesNumber = $"DS-{sale.Id:D10}-{DateTime.Now.Year.ToString()}";
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message, "Error adding Sale");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(DailySaleDto saleDto)
    {
        var existingSale = await _context.DailySales
            .FirstOrDefaultAsync(x => x.Id == saleDto.Id && !x.IsDeleted);

        if (existingSale == null)
            return false;

        try
        {
            // Map top-level props
            _mapper.Map(saleDto, existingSale);

            existingSale.UpdatedAt = DateTime.UtcNow;
            existingSale.UpdatedBy = GetCurrentUsername();
            existingSale.ExpectedPaymentDate = CalculateExpectedPaymentDate(
                saleDto.PaymentTermsOption ?? PaymentTermsOption.Today,
                existingSale.DateOfSales,
                saleDto.CustomPaymentTermsOption ?? 0);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update sale");
            return false;
        }
    }


    public async Task<bool> DeleteSaleASync(int id)
    {
        var existingSale = await _context.DailySales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (existingSale == null)
        {
            return false;
        }
        try
        {
            existingSale.IsDeleted = true;
            existingSale.DeletedAt = DateTime.UtcNow;
            _ = _context.DailySales.Update(existingSale);
            int result = await _context.SaveChangesAsync();
            return result > 0;

        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message}");
            return false;

        }
    }

    private DateTime CalculateExpectedPaymentDate(PaymentTermsOption terms, DateTime dateSales, int customDate = 0)
    {
        return terms switch
        {
            PaymentTermsOption.Today => dateSales,
            PaymentTermsOption.SevenDays => dateSales.AddDays(7),
            PaymentTermsOption.ThirtyDays => dateSales.AddDays(30),
            PaymentTermsOption.SixtyDays => dateSales.AddDays(60),
            PaymentTermsOption.NinetyDays => dateSales.AddDays(90),
            PaymentTermsOption.Custom => dateSales.AddDays(customDate),
            _ => dateSales
        };
    }
}
public interface ISalesService
{
    Task<List<DailySaleDto>> GetAllDailySalesAsync();
    Task<List<DailySaleDto>> GetAllDailySalesByDaySetAsync(DateTime date);
    Task<List<DailySaleDto>> GetAllDailySaleByDaysAsync(DateRangeOption range);

    Task<List<DailySaleDto>> GetAllDailySalesPaidAsync(DateTime date, BranchOption branch);
    Task<List<DailySaleDto>> GetAllDailySalesUnpaidAsync(DateTime date, BranchOption branch);
    Task<List<DailySaleDto>> GetAllCollectionAsync(DateTime date, BranchOption branch);
    Task<decimal> GetAllDailyInvoiceAsync(DateTime date);
    Task<decimal> GetAllDailyNonVoiceAsync(DateTime date);


    Task<DailySaleDto?> GetDailySaleByIdAsync(int id);
    Task<bool> AddAsync(DailySaleDto saleDto);
    Task<bool> UpdateAsync(DailySaleDto saleDto);
    Task<bool> DeleteSaleASync(int id);
}
