using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GenstarXKulayInventorySystem.Server.Services;

public class PurchaseOrderItemService: IPurchaseOrderItemService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PurchaseOrderItemService> _logger;
    public PurchaseOrderItemService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<PurchaseOrderItemService> logger)
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

    public async Task<List<PurchaseOrderItemDto>> GetAllUnrecieveItemsAsync(int purchaseOrderId)
    {
        List<PurchaseOrderItem> purchaseOrderItems = await _context.PurchaseOrderItems
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.PurchaseOrder)
            .Include(c => c.Product)
            .Include(c => c.ProductBrand)
            .Where(e => !e.IsDeleted && e.PurchaseOrderId == purchaseOrderId && !e.IsRecieved)
            .OrderByDescending(e => e.Id).ToListAsync();
        if (purchaseOrderItems == null || purchaseOrderItems.Count == 0)
        {
            return new List<PurchaseOrderItemDto>();
        }
        List<PurchaseOrderItemDto> purchaseOrderItemDtos = _mapper.Map<List<PurchaseOrderItemDto>>(purchaseOrderItems);
        return purchaseOrderItemDtos;
    }


    public async Task<List<PurchaseOrderItemDto>> GetAllRecieveItemsAsync(int purchaseOrderId)
    {
        List<PurchaseOrderItem> purchaseOrderItems = await _context.PurchaseOrderItems
           .AsNoTracking()
           .AsSplitQuery() 
           .Include(c => c.Product)
           .Include(c => c.ProductBrand)
           .Where(e => !e.IsDeleted && e.PurchaseOrderId == purchaseOrderId && e.IsRecieved)
           .OrderByDescending(e => e.Id).ToListAsync();
        if (purchaseOrderItems == null || purchaseOrderItems.Count == 0)
        {
            return new List<PurchaseOrderItemDto>();
        }
        List<PurchaseOrderItemDto> purchaseOrderItemDtos = _mapper.Map<List<PurchaseOrderItemDto>>(purchaseOrderItems);
        return purchaseOrderItemDtos;
    }

    public async Task<List<PurchaseOrderItemDto>> GetAllUnrecieveItemsByDate()
    {
        List<PurchaseOrderItem> purchaseOrderItems = await _context.PurchaseOrderItems
                                                     .AsNoTracking()
                                                     .AsSplitQuery()
                                                     .Include(c => c.PurchaseOrder)
                                                     .Include(c => c.Product)
                                                     .Include(c => c.ProductBrand)
                                                     .Where(e => !e.IsDeleted && e.IsRecieved)
                                                     .ToListAsync();
        if (purchaseOrderItems == null || purchaseOrderItems.Count == 0)
            {
            return new List<PurchaseOrderItemDto>();
        }
        List<PurchaseOrderItemDto> purchaseOrderItemDtos = _mapper.Map<List<PurchaseOrderItemDto>>(purchaseOrderItems);
        return purchaseOrderItemDtos;
    }


    public async Task<PurchaseOrderItemDto?> GetByIdAsync(int id)
    {
        var purchaseOrderItem = await _context.PurchaseOrderItems
            .AsNoTracking()
            .Include(c => c.PurchaseOrder)
            .Include(c => c.Product)
            .Include(c => c.ProductBrand)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        return purchaseOrderItem == null ? null : _mapper.Map<PurchaseOrderItemDto>(purchaseOrderItem);
    }

    public async Task<bool> AddAsync(PurchaseOrderItemDto purchaseOrderItemDto)
    {
        try
        {
            var existingPurchaseOrderItem = await _context.PurchaseOrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemDescription == purchaseOrderItemDto.ItemDescription && x.PurchaseOrderId == purchaseOrderItemDto.PurchaseOrderId);
            if (existingPurchaseOrderItem != null)
                return false;
            var purchaseOrderItem = _mapper.Map<PurchaseOrderItem>(purchaseOrderItemDto);
            purchaseOrderItem.CreatedBy = GetCurrentUsername();
            await _context.PurchaseOrderItems.AddAsync(purchaseOrderItem);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding Purchase Order Item");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(PurchaseOrderItemDto purchaseOrderItemDto)
    {
        try
        {
            var existingPurchaseOrderItem = await _context.PurchaseOrderItems
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == purchaseOrderItemDto.Id);
            if (existingPurchaseOrderItem == null)
                return false;
            var purchaseOrderItem = _mapper.Map<PurchaseOrderItem>(purchaseOrderItemDto);
            purchaseOrderItem.UpdatedBy = GetCurrentUsername();
            purchaseOrderItem.UpdatedAt = DateTime.UtcNow;
            _context.PurchaseOrderItems.Update(purchaseOrderItem);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Purchase Order Item");
            return false;
        }
    }

    public async Task<bool> UpdateItemsAsync(List<PurchaseOrderItemDto> purchaseItems)
    {
        try
        {
            if (purchaseItems == null || !purchaseItems.Any())
                return false;

            var itemIds = purchaseItems.Select(i => i.Id).ToList();
            var existingItems = await _context.PurchaseOrderItems.Include(e => e.Product)
                .Where(x => itemIds.Contains(x.Id) && !x.IsDeleted)
                .ToListAsync();

            foreach (var dto in purchaseItems)
            {
                var entity = existingItems.FirstOrDefault(x => x.Id == dto.Id);
                if (entity != null)
                {
                    entity.ItemQuantity = dto.ItemQuantity;
                    entity.PurchaseItemMeasurementOption = dto.PurchaseItemMeasurementOption;
                    entity.ItemAmount = dto.ItemAmount ?? 0;
                    entity.ItemDescription = dto.ItemDescription;
                    entity.IsRecieved = dto.IsRecieved;
                    entity.UpdatedBy = GetCurrentUsername();
                    entity.UpdatedAt = DateTime.UtcNow;
                    if (dto.IsRecieved && entity.ProductId.HasValue && entity.Product != null)
                    {
                        entity.Product.Quantity += dto.ItemQuantity;
                        entity.Product.ActualQuantity += dto.ItemQuantity;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Purchase Order Items");
            return false;
        }
    }



    public async Task<bool> MarkAsRecievedAsync(List<PurchaseOrderDto> dtos)
    {
        try
        {
            var purchaseOrderIds = dtos.Select(dto => dto.Id).ToList();

            var itemsToUpdate = await _context.PurchaseOrderItems
                .Where(item => purchaseOrderIds.Contains(item.PurchaseOrderId.Value) && !item.IsDeleted)
                .ToListAsync();

            if (!itemsToUpdate.Any())
                return false;

            foreach (var item in itemsToUpdate)
            {
                item.IsRecieved = true;
                item.UpdatedBy = GetCurrentUsername();
                item.UpdatedAt = DateTime.UtcNow;
            }

            _context.PurchaseOrderItems.UpdateRange(itemsToUpdate);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking Purchase Order Items as received");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var purchaseOrderItem = await _context.PurchaseOrderItems
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (purchaseOrderItem == null)
                return false;
            purchaseOrderItem.IsDeleted = true;
            purchaseOrderItem.UpdatedBy = GetCurrentUsername();
            purchaseOrderItem.UpdatedAt = DateTime.UtcNow;
            _context.PurchaseOrderItems.Update(purchaseOrderItem);
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Purchase Order Item");
            return false;
        }
    }   
}

public interface  IPurchaseOrderItemService
{
    Task<List<PurchaseOrderItemDto>> GetAllUnrecieveItemsAsync(int purchaseOrderId);
    Task<List<PurchaseOrderItemDto>> GetAllRecieveItemsAsync(int purchaseOrderId);
    Task<List<PurchaseOrderItemDto>> GetAllUnrecieveItemsByDate();
    Task<PurchaseOrderItemDto?> GetByIdAsync(int id);
    Task<bool> AddAsync(PurchaseOrderItemDto purchaseOrderItemDto);
    Task<bool> UpdateAsync(PurchaseOrderItemDto purchaseOrderItemDto);
    Task<bool> UpdateItemsAsync(List<PurchaseOrderItemDto> purchaseItems);
    Task<bool> MarkAsRecievedAsync(List<PurchaseOrderDto> dtos);
    Task<bool> DeleteAsync(int id);
}
