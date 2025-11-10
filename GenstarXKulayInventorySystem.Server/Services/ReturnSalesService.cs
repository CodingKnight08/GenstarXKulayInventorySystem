using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Services;

public class ReturnSalesService:IReturnSalesService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ReturnSalesService> _logger;
    public ReturnSalesService(
        InventoryDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ReturnSalesService> logger)
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

    public async Task<List<ReturnSalesDto>> GetAllReturnSalesAsync(BranchOption branch, DateTime date)
    {
        var (start, end) = PhilippineTime.GetDayRange(date);

        var returnSales = await _context.ReturnSales
            .Include(rs => rs.DailySale)
            .Where(rs => rs.Branch == branch &&
                         rs.DateOfReturn >= start &&
                         rs.DateOfReturn <= end)
            .OrderByDescending(rs => rs.DateOfReturn)
            .ToListAsync();

        return _mapper.Map<List<ReturnSalesDto>>(returnSales);
    }

    public async Task<ReturnSalesDto> GetReturnSaleById(int id)
    {
        var returnSale = await _context.ReturnSales
            .Include(rs => rs.DailySale)
            .FirstOrDefaultAsync(rs => rs.Id == id);
        if (returnSale == null)
        {
            _logger.LogWarning("Return sale with ID {Id} not found.", id);
            return new ReturnSalesDto();
        }
        return _mapper.Map<ReturnSalesDto>(returnSale);
    }

    public async Task<bool> CreateReturnSale(ReturnSalesDto returnSalesDto)
    {
        try
        {
            var returnSale = _mapper.Map<ReturnSale>(returnSalesDto);
            returnSale.CreatedBy = GetCurrentUsername();
            _context.ReturnSales.Add(returnSale);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating return sale.");
            return false;
        }
    }


    public async Task<bool> DeleteReturnSale(int id)
    {
        try
        {
            var returnSale = await _context.ReturnSales.FindAsync(id);
            if (returnSale == null)
            {
                _logger.LogWarning("Return sale with ID {Id} not found for deletion.", id);
                return false;
            }

            returnSale.IsDeleted = true;
            returnSale.DeletedAt = PhilippineTime.Now; 

            _context.ReturnSales.Update(returnSale);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Return sale with ID {Id} marked as deleted at {DeletedAt}.", id, returnSale.DeletedAt);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting return sale with ID {Id}.", id);
            return false;
        }
    }


}
public interface IReturnSalesService
{
    Task<List<ReturnSalesDto>> GetAllReturnSalesAsync(BranchOption branch, DateTime date);
    Task<ReturnSalesDto> GetReturnSaleById(int id);
    Task<bool> CreateReturnSale(ReturnSalesDto returnSalesDto);
    Task<bool> DeleteReturnSale(int id);
}
