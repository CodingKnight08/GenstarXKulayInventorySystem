using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GenstarXKulayInventorySystem.Server.Services;

public class SupplierService:ISupplierService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SupplierService> _logger;
    public SupplierService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<SupplierService> logger)
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

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        List<Supplier> suppliers = await _context.Suppliers
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => !e.IsDeleted)
            .ToListAsync();
        if (suppliers == null || suppliers.Count == 0)
        {
            return new List<SupplierDto>();
        }
        List<SupplierDto> supplierDtos = _mapper.Map<List<SupplierDto>>(suppliers);
        return supplierDtos;
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return supplier == null ? null : _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<bool> AddAsync(SupplierDto supplierDto)
    {
        try
        {
            var existingSupplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SupplierName == supplierDto.SupplierName);
            if (existingSupplier != null)
                return false;
            var supplier = _mapper.Map<Supplier>(supplierDto);
            supplier.CreatedBy = GetCurrentUsername();
            supplier.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            await _context.Suppliers.AddAsync(supplier);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding supplier: {SupplierName}", supplierDto.SupplierName);
            return false;
        }
    }
    public async Task<SupplierDto?> CreateAndReturnAsync(SupplierDto supplierDto)
    {
        try
        {
            var existingSupplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SupplierName == supplierDto.SupplierName);

            if (existingSupplier != null)
                return null;

            var supplier = _mapper.Map<Supplier>(supplierDto);
            supplier.CreatedBy = GetCurrentUsername();
            supplier.CreatedAt = UtilitiesHelper.GetPhilippineTime();

            await _context.Suppliers.AddAsync(supplier);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return _mapper.Map<SupplierDto>(supplier);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding supplier: {SupplierName}", supplierDto.SupplierName);
            return null;
        }
    }


    public async Task<bool> UpdateAsync(SupplierDto supplierDto)
    {
       var existingSupplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == supplierDto.Id && !x.IsDeleted);
        if (existingSupplier == null)
            return false;
        try
        {
            var supplier = _mapper.Map<Supplier>(supplierDto);
            supplier.UpdatedBy = GetCurrentUsername();
            supplier.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
            _context.Suppliers.Update(supplier);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "Error updating supplier with ID {Id}", supplierDto.Id);

            return false;
        }
    }
    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        if (supplier == null)
            return false;
        try
        {
            supplier.IsDeleted = true;
            
            supplier.DeletedAt = UtilitiesHelper.GetPhilippineTime();
            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting supplier with ID {Id}", id);
            // Log the exception
            return false;
        }
    }


}
public interface ISupplierService
{
    // Define methods for supplier operations
    Task<List<SupplierDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<bool> AddAsync(SupplierDto supplierDto);
    Task<SupplierDto?> CreateAndReturnAsync(SupplierDto supplierDto);
    Task<bool> UpdateAsync(SupplierDto supplierDto);
    Task<bool> DeleteAsync(int id);
}