using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;


namespace GenstarXKulayInventorySystem.Server.Services;

public class SalesService:ISalesService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SalesService> _logger;

    public SalesService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<SalesService> logger)
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

    public async Task<List<DailySaleDto>> GetAllDailySalesAsync()
    {
        List<DailySale> dailySales = await _context.DailySales.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted && e.DateOfSales == UtilitiesHelper.GetPhilippineTime().Date).ToListAsync();
        if(dailySales == null || dailySales.Count == 0)
        {
            return new List<DailySaleDto>();
        }

        List<DailySaleDto> dailySaleDtos = _mapper.Map<List<DailySaleDto>>(dailySales);
        return dailySaleDtos;
    }
    public async Task<List<DailySaleDto>> GetAllDailySaleByDaysAsync(int days)
    {
        List<DailySale> dailySales = await _context.DailySales.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted && (e.DateOfSales <= DateTime.Now.AddDays(-days))).ToListAsync();
        if (dailySales == null || dailySales.Count == 0)
        {
            return new List<DailySaleDto>();
        }

        List<DailySaleDto> dailySaleDtos = _mapper.Map<List<DailySaleDto>>(dailySales);
        return dailySaleDtos;
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
            sale.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            sale.CreatedBy = GetCurrentUsername();
            _ = await _context.DailySales.AddAsync(sale);
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
    Task<List<DailySaleDto>> GetAllDailySaleByDaysAsync(int days);
    Task<DailySaleDto?> GetDailySaleByIdAsync(int id);
    Task<bool> AddAsync(DailySaleDto saleDto);
    Task<bool> UpdateAsync(DailySaleDto saleDto);
    Task<bool> DeleteSaleASync(int id);
}
