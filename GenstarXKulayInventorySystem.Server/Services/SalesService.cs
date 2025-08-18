using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
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
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
    }

    public async Task<List<DailySaleDto>> GetAllDailySalesAsync()
    {
        List<DailySale> dailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted && e.DateOfSales.Date == UtilitiesHelper.GetPhilippineTime().Date)
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
        DateTime chosenDate = date.Date; 
        DateTime startOfDay = chosenDate;
        DateTime endOfDay = chosenDate.AddDays(1);

        List<DailySale> dailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted && e.DateOfSales >= startOfDay && e.DateOfSales < endOfDay)
            .OrderByDescending(e => e.DateOfSales)
            .ToListAsync();
        if (dailySales == null || dailySales.Count == 0)
        {
            return new List<DailySaleDto>();
        }

        List<DailySaleDto> dailySaleDtos = _mapper.Map<List<DailySaleDto>>(dailySales);
        return dailySaleDtos;
    }

    public async Task<List<DailySaleDto>> GetAllDailySaleByDaysAsync(DateRangeOption range)
    {
        DateTime startDate = range switch
        {
            DateRangeOption.OneWeek => UtilitiesHelper.GetPhilippineTime().AddDays(-7),
            DateRangeOption.OneMonth => UtilitiesHelper.GetPhilippineTime().AddMonths(-1),
            DateRangeOption.TwoMonths => UtilitiesHelper.GetPhilippineTime().AddMonths(-2),
            DateRangeOption.ThreeMonths => UtilitiesHelper.GetPhilippineTime().AddMonths(-3),
            DateRangeOption.OneYear => UtilitiesHelper.GetPhilippineTime().AddYears(-1),
            _ => UtilitiesHelper.GetPhilippineTime() 
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

    
    public async Task<DailySaleDto?> GetDailySaleByIdAsync(int id)
    {
        var dailySale = await _context.DailySales.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return dailySale == null ? null : _mapper.Map<DailySaleDto>(dailySale);
    }

    public async Task<bool> AddAsync(DailySaleDto saleDto)
    {
        try
        {
            var existingSale = await _context.DailySales.AsNoTracking().FirstOrDefaultAsync(e => e.Branch == saleDto.Branch && e.RecieptReference == saleDto.RecieptReference);
            if (existingSale != null) {
                return false;
            }
            var sale = _mapper.Map<DailySale>(saleDto);
            sale.DateOfSales = UtilitiesHelper.GetPhilippineTime();
            sale.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            sale.CreatedBy = GetCurrentUsername();
            sale.TotalAmount = saleDto.SaleItems.Sum(x => (x.ItemPrice) * (x.Quantity));
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
        var existingSale = await _context.DailySales.AsNoTracking().FirstOrDefaultAsync(x => x.Id == saleDto.Id && !x.IsDeleted);
        if (existingSale == null)
        {
            return false;
        }
        try
        {
            var sale = _mapper.Map<DailySale>(saleDto);
            sale.UpdatedAt = UtilitiesHelper.GetPhilippineTime();   
            sale.UpdatedBy = GetCurrentUsername();
            _ = _context.DailySales.Update(sale);
            int result = await _context.SaveChangesAsync();
            return result > 0;

        }
        catch (Exception ex) { 
            _logger.LogError($"{ex.Message}");
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
            existingSale.DeletedAt = UtilitiesHelper.GetPhilippineTime();
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
}
public interface ISalesService
{
    Task<List<DailySaleDto>> GetAllDailySalesAsync();
    Task<List<DailySaleDto>> GetAllDailySalesByDaySetAsync(DateTime date);
    Task<List<DailySaleDto>> GetAllDailySaleByDaysAsync(DateRangeOption range);
    Task<DailySaleDto?> GetDailySaleByIdAsync(int id);
    Task<bool> AddAsync(DailySaleDto saleDto);
    Task<bool> UpdateAsync(DailySaleDto saleDto);
    Task<bool> DeleteSaleASync(int id);
}
