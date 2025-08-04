using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GenstarXKulayInventorySystem.Server.Services;

public class PurchaseOrderService:IPurchaseOrderService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PurchaseOrderService> _logger;
    public PurchaseOrderService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<PurchaseOrderService> logger)
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
    public async Task<List<PurchaseOrderDto>> GetAllAsync()
    {
        List<PurchaseOrder> purchaseOrders = await _context.PurchaseOrders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.Supplier)
            .Where(e => !e.IsDeleted && !e.IsRecieved && e.PurchaseRecieveOption != OrdersHelper.PurchaseRecieveOption.RecieveAll)
            .OrderByDescending(e => e.PurchaseOrderDate).ToListAsync();
        if (purchaseOrders == null || purchaseOrders.Count == 0)
        {
            return new List<PurchaseOrderDto>();
        }
        List<PurchaseOrderDto> purchaseOrderDtos = _mapper.Map<List<PurchaseOrderDto>>(purchaseOrders);
        return purchaseOrderDtos;
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .AsNoTracking()
            .Include(c => c.Supplier)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted && !e.IsRecieved);
        return purchaseOrder == null ? null : _mapper.Map<PurchaseOrderDto>(purchaseOrder);
    }

    public async Task<bool> AddAsync(PurchaseOrderDto purchaseOrderDto)
    {
        try
        {
            var existingPurchaseOrder = await _context.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PurchaseOrderNumber == purchaseOrderDto.PurchaseOrderNumber);
            if (existingPurchaseOrder != null)
                return false;
            var purchaseOrder = _mapper.Map<PurchaseOrder>(purchaseOrderDto);
            purchaseOrder.CreatedBy = GetCurrentUsername();
            purchaseOrder.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            foreach (var item in purchaseOrder.PurchaseOrderItems)
            {
                item.PurchaseOrder = null;
                item.ProductBrand = null;
                item.Product = null;
                item.CreatedBy = purchaseOrder.CreatedBy;
                item.CreatedAt = purchaseOrder.PurchaseOrderDate;
            }
            await _context.PurchaseOrders.AddAsync(purchaseOrder);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding Purchase Order");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(PurchaseOrderDto purchaseOrderDto)
    {
        var existingPurchaseOrder = await _context.PurchaseOrders
            .Include(po => po.PurchaseOrderItems)
            .FirstOrDefaultAsync(x => x.Id == purchaseOrderDto.Id && !x.IsDeleted && !x.IsRecieved);

        if (existingPurchaseOrder == null)
            return false;

        try
        {
            // Update purchase order basic fields
            existingPurchaseOrder.UpdatedBy = GetCurrentUsername();
            existingPurchaseOrder.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
            existingPurchaseOrder.SupplierId = purchaseOrderDto.SupplierId;
            existingPurchaseOrder.PurchaseOrderNumber = purchaseOrderDto.PurchaseOrderNumber;
            existingPurchaseOrder.PurchaseOrderDate = purchaseOrderDto.PurchaseOrderDate;
            existingPurchaseOrder.ExpectedDeliveryDate = purchaseOrderDto.ExpectedDeliveryDate;


            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Purchase Order: {PurchaseOrderNumber}", purchaseOrderDto.PurchaseOrderNumber);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.PurchaseOrderItems) // Include related items
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && !x.IsRecieved);

        if (purchaseOrder == null)
            return false;

        try
        {
            // Soft delete the purchase order
            purchaseOrder.IsDeleted = true;
            purchaseOrder.DeletedAt = UtilitiesHelper.GetPhilippineTime();

            // Soft delete each associated purchase order item
            foreach (var item in purchaseOrder.PurchaseOrderItems.Where(i => !i.IsDeleted))
            {
                item.IsDeleted = true;
                item.DeletedAt = UtilitiesHelper.GetPhilippineTime();
            }

            _context.PurchaseOrders.Update(purchaseOrder);

            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Purchase Order: {PurchaseOrderNumber}", purchaseOrder.PurchaseOrderNumber);
            return false;
        }
    }

}
public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderDto>> GetAllAsync();
    Task<PurchaseOrderDto?> GetByIdAsync(int id);
    Task<bool> AddAsync(PurchaseOrderDto purchaseOrderDto);
    Task<bool> UpdateAsync(PurchaseOrderDto purchaseOrderDto);
    Task<bool> DeleteAsync(int id);
}
