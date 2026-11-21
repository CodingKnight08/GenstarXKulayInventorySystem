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
            .Include(c => c.BranchProduct)
            .ThenInclude(gb => gb.MasterProduct)
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
           .Include(c => c.BranchProduct)
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
                                                     .Include(c => c.BranchProduct)
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
            .Include(c => c.BranchProduct)
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

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var itemIds = purchaseItems
                        .Select(x => x.Id)
                        .Distinct()
                        .ToList();

                    var existingItems = await _context.PurchaseOrderItems
                        .Include(x => x.BranchProduct)
                        .Where(x => itemIds.Contains(x.Id) && !x.IsDeleted)
                        .ToListAsync();

                    if (existingItems.Count != itemIds.Count)
                    {
                        throw new InvalidOperationException(
                            "One or more purchase order items could not be found.");
                    }

                    foreach (var dto in purchaseItems)
                    {
                        var entity = existingItems.First(x => x.Id == dto.Id);

                        // Prevent changing an item that has already been received.
                        if (entity.IsRecieved && !dto.IsRecieved)
                        {
                            throw new InvalidOperationException(
                                $"Item {entity.Id} has already been received and cannot be marked as unreceived.");
                        }

                        if (entity.IsRecieved &&
                            entity.ItemQuantity != dto.ItemQuantity)
                        {
                            throw new InvalidOperationException(
                                $"Cannot change the quantity of already received item {entity.Id}.");
                        }

                        // Update item details.
                        entity.PurchaseItemMeasurementOption =
                            dto.PurchaseItemMeasurementOption;

                        entity.ItemAmount = dto.ItemAmount ?? 0;
                        entity.ItemDescription = dto.ItemDescription;

                        // Add inventory only when transitioning to received.
                        if (!entity.IsRecieved && dto.IsRecieved)
                        {
                            if (entity.BranchProductId.HasValue)
                            {
                                if (entity.BranchProduct == null)
                                {
                                    throw new InvalidOperationException(
                                        $"Branch product not found for purchase order item {entity.Id}.");
                                }

                                entity.BranchProduct.ActualQuantity += dto.ItemQuantity;
                            }

                            entity.ItemQuantity = dto.ItemQuantity;
                            entity.IsRecieved = true;
                        }

                        entity.UpdatedBy = GetCurrentUsername();
                        entity.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase order items.");
            return false;
        }
    }





    public async Task<bool> MarkAsRecievedAsync(List<PurchaseOrderDto> dtos)
    {
        try
        {
            if (dtos == null || !dtos.Any())
                return false;

            var purchaseOrderIds = dtos
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync();

                try
                {
                    var itemsToUpdate = await _context.PurchaseOrderItems
                        .Include(x => x.BranchProduct)
                        .Where(item =>
                            item.PurchaseOrderId.HasValue &&
                            purchaseOrderIds.Contains(item.PurchaseOrderId.Value) &&
                            !item.IsDeleted &&
                            !item.IsRecieved)
                        .ToListAsync();

                    foreach (var item in itemsToUpdate)
                    {
                        if (item.BranchProductId.HasValue)
                        {
                            if (item.BranchProduct == null)
                            {
                                throw new InvalidOperationException(
                                    $"Branch product not found for purchase order item {item.Id}.");
                            }

                            item.BranchProduct.ActualQuantity += item.ItemQuantity;
                        }

                        item.IsRecieved = true;
                        item.UpdatedBy = GetCurrentUsername();
                        item.UpdatedAt = DateTime.UtcNow;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking purchase order items as received.");
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
