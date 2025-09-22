using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;


namespace GenstarXKulayInventorySystem.Server.Services;

public class SaleItemService:ISaleItemService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SaleItemService> _logger;

    public SaleItemService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<SaleItemService> logger)
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

    public async Task<List<SaleItemDto>> GetAllSaleItemsAsync(int dailySaleId, int skip, int take)
    {
        List<SaleItem> saleItems = await _context.SaleItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => e.DailySaleId == dailySaleId && !e.IsDeleted)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        if(saleItems == null || saleItems.Count == 0)
        {
            return new List<SaleItemDto>();
        }

        List<SaleItemDto> saleItemDtos = _mapper.Map<List<SaleItemDto>>(saleItems); 
        return saleItemDtos;
    }

    public async Task<SaleItemPageResultDto<SaleItemDto>> GetAllSaleItemsPageAsync(int dailySaleId, int skip, int take)
    {
        var query = _context.SaleItems
            .AsNoTracking()
            .AsSplitQuery()
            .Where(e => e.DailySaleId == dailySaleId && !e.IsDeleted);

        int totalCount = await query.CountAsync();

        List<SaleItem> saleItems = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var saleItemDtos = _mapper.Map<List<SaleItemDto>>(saleItems);

        return new SaleItemPageResultDto<SaleItemDto>
        {
            SaleItems = saleItemDtos,
            TotalCount = totalCount
        };
    }


    public async Task<List<SaleItemDto>> GetAllUndeductedItemsAsync()
    {
        List<SaleItem> salesItems = await _context.SaleItems
            .AsNoTracking()
            .AsSplitQuery()
            .Include(si=> si.Product)
            .Where(si => !si.IsDeleted && !si.IsDeducted).ToListAsync();
        if(salesItems == null || salesItems.Count == 0)
        {
            return new List<SaleItemDto>();
        }
        List<SaleItemDto> saleItemsDto = _mapper.Map<List<SaleItemDto>>(salesItems);
        return saleItemsDto;
    }

    public async Task<SaleItemDto?> GetSaleItemById(int saleItemId)
    {
        var saleItem = await _context.SaleItems.FirstOrDefaultAsync(e => e.Id == saleItemId);
        return saleItem == null ? null: _mapper.Map<SaleItemDto>(saleItem);
    }

    public async Task<bool> AddSaleItemAsync(SaleItemDto saleItemDto)
    {
        try
        {
            var existingSale = await _context.SaleItems.AsNoTracking().FirstOrDefaultAsync(e => e.Id == saleItemDto.Id && e.CreatedAt == UtilitiesHelper.GetPhilippineTime());
            if (existingSale != null) {
                return false;
            }
            var saleItem = _mapper.Map<SaleItem>(saleItemDto);
            saleItem.CreatedAt = DateTime.UtcNow;
            saleItem.CreatedBy = GetCurrentUsername();
            _ = await _context.SaleItems.AddAsync(saleItem);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex) {
            _logger.LogError(ex.Message, "Error in adding Sale Item");
            return false;
        }
    }

    public async Task<bool> UpdateSaleItemAsync(SaleItemDto saleItem)
    {
        var existingSaleItem = await _context.SaleItems.AsNoTracking().FirstOrDefaultAsync(e => e.Id == saleItem.Id && !e.IsDeleted);
        if (existingSaleItem == null)
        {
            return false;
        }

        try
        {
            var saleItems = _mapper.Map<SaleItem>(saleItem);
            saleItem.UpdatedAt = DateTime.UtcNow;
            saleItem.UpdatedBy = GetCurrentUsername();
            _ = _context.SaleItems.Update(saleItems);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in updating saleItem: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateSaleItemStatus(SaleItemDto saleItem)
    {
        var entity = new SaleItem
        {
            Id = saleItem.Id,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = GetCurrentUsername(),
            IsDeducted = true
        };

        _context.SaleItems.Attach(entity);
        _context.Entry(entity).Property(x => x.UpdatedAt).IsModified = true;
        _context.Entry(entity).Property(x => x.UpdatedBy).IsModified = true;
        _context.Entry(entity).Property(x => x.IsDeducted).IsModified = true;

        return await _context.SaveChangesAsync() > 0;
    }


    public async Task<bool> DeleteSaleItemAsync(int saleItemId)
    {
        var existingSaleItem = await _context.SaleItems.AsNoTracking().FirstOrDefaultAsync(e => e.Id == saleItemId);
        if (existingSaleItem == null)
        {
            return false;
        }
        try
        {
            existingSaleItem.IsDeleted = true;
            existingSaleItem.DeletedAt = DateTime.UtcNow;
            _ = _context.SaleItems.Update(existingSaleItem);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{ex.Message}, Error occured in deleting Sale Item");
            return false ;
        }
    }
}
public interface ISaleItemService
{
    Task<List<SaleItemDto>> GetAllSaleItemsAsync(int dailySaleId, int skip, int take);
    Task<SaleItemPageResultDto<SaleItemDto>> GetAllSaleItemsPageAsync(int dailySaleId, int skip, int take);
    Task<List<SaleItemDto>> GetAllUndeductedItemsAsync();
    Task<SaleItemDto?> GetSaleItemById(int saleItemId);
    Task<bool> AddSaleItemAsync(SaleItemDto saleItemDto);
    Task<bool> UpdateSaleItemAsync(SaleItemDto saleItem);
    Task<bool> UpdateSaleItemStatus(SaleItemDto saleItem);
    Task<bool> DeleteSaleItemAsync(int saleItemId);
}
