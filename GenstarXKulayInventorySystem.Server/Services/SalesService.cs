using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Extensions;
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
            .Where(e => !e.IsDeleted && e.DateOfSales.Date == DateTime.Now.Date)
            .OrderByDescending(e => e.DateOfSales)
            .ToListAsync();
        if(dailySales == null || dailySales.Count == 0)
        {
            return new List<DailySaleDto>();
        }

        List<DailySaleDto> dailySaleDtos = _mapper.Map<List<DailySaleDto>>(dailySales);
        return dailySaleDtos;
    }

    public async Task<List<DailySaleDto>> GetAllDailySaleByBranch(BranchOption branch, DateTime dateTime)
    {
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(dateTime);

        var dailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted
                     && e.Branch == branch
                     && e.DateOfSales >= startOfDay
                     && e.DateOfSales < endOfDay)
            .OrderByDescending(e => e.DateOfSales)
            .ToListAsync();

        if (dailySales == null || dailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(dailySales);
    }




    public async Task<List<DailySaleDto>> GetAllDailySalesByDaySetAsync(DateTime date)
    {
        var chosenDateLocal = date.Date;

        // assume the chosenDate is local, so convert to UTC
        var startOfDay = chosenDateLocal.Date;
        var endOfDay = chosenDateLocal.AddDays(1);

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
            DateRangeOption.OneWeek => DateTime.Now.AddDays(-7),
            DateRangeOption.OneMonth => DateTime.Now.AddMonths(-1),
            DateRangeOption.TwoMonths => DateTime.Now.AddMonths(-2),
            DateRangeOption.ThreeMonths => DateTime.Now.AddMonths(-3),
            DateRangeOption.OneYear => DateTime.Now.AddYears(-1),
            _ => DateTime.Now 
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
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(date);

        var paidDailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Include(ds => ds.SaleItems)
            .ThenInclude(bp => bp.BranchProduct)
            .Where(ds => !ds.IsDeleted
                      && ds.IsApproved
                      && ds.Branch == branch
                      && ds.PaymentType != null
                      && ds.DateOfSales >= startOfDay
                      && ds.DateOfSales < endOfDay)
            .ToListAsync();

        if (paidDailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(paidDailySales);
    }



    public async Task<decimal> GetAllDailyInvoiceAsync(DateTime date)
    {
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(date);

        decimal invoices = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.SalesOption == PurchaseRecieptOption.BIR
                      && ds.DateOfSales >= startOfDay
                      && ds.DateOfSales < endOfDay)
            .SumAsync(ds => (decimal?)ds.TotalAmount) ?? 0;

        return invoices;
    }

    public async Task<decimal> GetAllDailyNonVoiceAsync(DateTime date)
    {
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(date);

        decimal nonVoices = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.SalesOption == PurchaseRecieptOption.NonBIR
                      && ds.DateOfSales >= startOfDay
                      && ds.DateOfSales < endOfDay)
            .SumAsync(ds => (decimal?)ds.TotalAmount) ?? 0;

        return nonVoices;
    }

    public async Task<List<DailySaleDto>> GetAllDailySalesUnpaidAsync(DateTime date, BranchOption branch)
    {
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(date);

        var unpaidDailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Include(s => s.SaleItems)
            .ThenInclude(bp => bp.BranchProduct)
            .Where(ds => !ds.IsDeleted
                      && !ds.IsPaid
                      && ds.IsApproved
                      && ds.Branch == branch
                      && ds.UpdatedAt == null
                      && ds.PaymentType == null
                      && ds.IsChargedSales)
            .ToListAsync();

        if (unpaidDailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(unpaidDailySales);
    }

    public async Task<List<DailySaleDto>> GetAllCollectionAsync(DateTime date, BranchOption branch)
    {
        var (startOfDay, endOfDay) = PhilippineTime.GetDayRange(date);

        var collectedSales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.IsPaid
                      && ds.IsApproved
                      && ds.Branch == branch
                      && ds.UpdatedAt.HasValue
                      && ds.UpdatedAt.Value >= startOfDay
                      && ds.UpdatedAt.Value < endOfDay)
            .ToListAsync();

        if (collectedSales == null || collectedSales.Count == 0)
            return new List<DailySaleDto>();

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
            var existingSale = await _context.DailySales
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Branch == saleDto.Branch && e.Id == saleDto.Id);

            if (existingSale != null)
                return false;

            var sale = _mapper.Map<DailySale>(saleDto);

            var phNow = PhilippineTime.Now;

            sale.DateOfSales = saleDto.DateOfSales;
            sale.CreatedAt = phNow;
            sale.CreatedBy = GetCurrentUsername();
            
            sale.TotalAmount = Math.Round(
                (saleDto.SaleItems?.Sum(x =>
                    (x.ItemPrice * x.Quantity * (x.Size ?? 1))
                ) ?? 0)
                + (saleDto.Commission ?? 0),
                2);

            sale.ExpectedPaymentDate = CalculateExpectedPaymentDate(
                saleDto.PaymentTermsOption ?? PaymentTermsOption.Today,
                phNow,
                saleDto.CustomPaymentTermsOption ?? 0);

            await _context.DailySales.AddAsync(sale);
            await _context.SaveChangesAsync();

            sale.SalesNumber = $"DS-{sale.Id:D10}-{phNow.Year}";
            int result = await _context.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding Sale");
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
            _mapper.Map(saleDto, existingSale);

            existingSale.UpdatedAt = PhilippineTime.Now;
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

    public async Task<bool> DeleteSaleAsync(int id)
    {
        var existingSale = await _context.DailySales
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (existingSale == null)
            return false;

        try
        {
            existingSale.IsDeleted = true;
            existingSale.DeletedAt = PhilippineTime.Now;

            _context.DailySales.Update(existingSale);

            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete sale");
            return false;
        }
    }

    private DateTime CalculateExpectedPaymentDate(
        PaymentTermsOption terms,
        DateTime dateSales,
        int customDate = 0)
    {
        var phDateSales = PhilippineTime.ToPH(dateSales);

        return terms switch
        {
            PaymentTermsOption.Today => phDateSales,
            PaymentTermsOption.SevenDays => phDateSales.AddDays(7),
            PaymentTermsOption.ThirtyDays => phDateSales.AddDays(30),
            PaymentTermsOption.SixtyDays => phDateSales.AddDays(60),
            PaymentTermsOption.NinetyDays => phDateSales.AddDays(90),
            PaymentTermsOption.Custom => phDateSales.AddDays(customDate),
            _ => phDateSales
        };
    }


}
public interface ISalesService
{
    Task<List<DailySaleDto>> GetAllDailySalesAsync();
    Task<List<DailySaleDto>> GetAllDailySaleByBranch(BranchOption branch, DateTime dateTime);
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
    Task<bool> DeleteSaleAsync(int id);
}
