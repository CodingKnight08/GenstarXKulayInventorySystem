using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Services;

public class WayBillService: IWayBillService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WayBillService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WayBillService(
        InventoryDbContext context,
        IMapper mapper,
        ILogger<WayBillService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }


    public async Task<List<WayBillDto>> GetAllWayBills(BranchOption branch)
    {
        var wayBills = await _context.WayBills
            .AsNoTracking()
            .AsSplitQuery()
            .Include(wb => wb.Supplier)
            .Include(wb => wb.WayBillItems)
            .ThenInclude(wi => wi.BranchProduct)
            .Where(wb => wb.Branch == branch)
            .OrderByDescending(wb => wb.CreatedAt)
            .ToListAsync();

        if (wayBills == null || wayBills.Count == 0)
        {
            _logger.LogInformation("No waybills found in the database.");
            return new List<WayBillDto>();
        }

        var wayBillDtos = _mapper.Map<List<WayBillDto>>(wayBills);

        foreach (var dto in wayBillDtos)
        {
            dto.TotalAmount = dto.WayBillItems.Sum(i => i.TotalPrice);
        }

        return wayBillDtos;
    }


    public async Task<WayBillDto?> GetWayBillById(int wayBill)
    {
        WayBill? wayBillEntity = await _context.WayBills
                            .AsNoTracking()
                            .AsSplitQuery()
                            .Include(wb => wb.Supplier)
                            .FirstOrDefaultAsync(wb => wb.Id == wayBill);
        if (wayBillEntity == null)
        {
            _logger.LogWarning("Waybill with ID {WayBillId} not found.", wayBill);
            return null;
        }
        WayBillDto wayBillDto = _mapper.Map<WayBillDto>(wayBillEntity);
        return wayBillDto;
    }

    public async Task<List<WayBillDto>> GetWayBillsBySupplierId(int supplierId)
    {
        List<WayBill> wayBills = await _context.WayBills
                            .AsNoTracking()
                            .AsSplitQuery()
                            .Include(wb => wb.Supplier)
                            .Include(wb => wb.WayBillItems)
                                .ThenInclude(wbi => wbi.BranchProduct)
                            .Where(wb => wb.SupplierId == supplierId)
                            .ToListAsync();
        if (wayBills == null || wayBills.Count == 0)
        {
            _logger.LogInformation("No waybills found for Supplier ID {SupplierId}.", supplierId);
            return new List<WayBillDto>();
        }
        List<WayBillDto> wayBillDtos = _mapper.Map<List<WayBillDto>>(wayBills);
        return wayBillDtos;
    }

    public async Task<List<WayBillItemsDto>> GetAllWayBillItemsByWayBillId(int waybillId)
    {
        List<WayBillItems> waybillItems = await _context.WayBillItems
                                            .AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(wb => wb.BranchProduct)
                                            .ThenInclude(wb => wb.MasterProduct)
                                            .Where(wb => wb.WayBillId == waybillId)
                                            .ToListAsync();
        if (waybillItems.Count == 0 || waybillItems is null)
        {
            _logger.LogInformation("No waybill items found {WaybillId}", waybillId);
            return new List<WayBillItemsDto>();
        }
        List<WayBillItemsDto> wayBillItems = _mapper.Map<List<WayBillItemsDto>>(waybillItems);
        return wayBillItems;
    }

    public async Task<bool> CreateWayBill(WayBillDto wayBillDto)
    {
        try
        { 

            var exist = await _context.WayBills
                            .AsNoTracking()
                            .FirstOrDefaultAsync(wb => wb.WayBillNumber == wayBillDto.WayBillNumber && !wb.IsDeleted);
            if (exist != null)
            {
                return false;
            }
            WayBill wayBillEntity = _mapper.Map<WayBill>(wayBillDto);
            wayBillEntity.DateReceived = PhilippineTime.Now; 
            wayBillEntity.CreatedBy = GetCurrentUsername();
            wayBillEntity.CreatedAt = PhilippineTime.Now;
            await _context.WayBills.AddAsync(wayBillEntity);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating waybill.");
            return false;
        }
    }


    public async Task<bool> UpdateWayBill(WayBillDto waybill)
    {
        var existingWayBill = await _context.WayBills.AsNoTracking().FirstOrDefaultAsync(wb => wb.Id == waybill.Id && !wb.IsDeleted);
        if (existingWayBill == null)
            return false;
        try
        {
            var wayBill = _mapper.Map<WayBill>(waybill);
            waybill.UpdatedBy = GetCurrentUsername();
            waybill.UpdatedAt = PhilippineTime.Now;
            _context.WayBills.Update(wayBill);
            int result = await _context.SaveChangesAsync();
            return result > 0;
                
         }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message, "Error updating waybill with ID {Id}", waybill.Id);
            return false;
        }
    }
    public async Task<bool> DeleteWayBill(int wayBillId)
    {
        try
        {
            var wayBillEntity = await _context.WayBills
                                    .Include(wb => wb.WayBillItems)
                                    .FirstOrDefaultAsync(wb => wb.Id == wayBillId);
            if (wayBillEntity == null)
            {
                _logger.LogWarning("Waybill with ID {WayBillId} not found for deletion.", wayBillId);
                return false;
            }
            wayBillEntity.IsDeleted = true;
            _context.WayBills.Update(wayBillEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting waybill with ID {WayBillId}.", wayBillId);
            return false;
        }
    }

    public async Task<List<WayBillDamageItemDto>> GetAllDamageItems(int waybillId)
    {
        try
        {
            var items = await _context.WayBillDamageItems
                .AsSplitQuery()
                .AsNoTracking()
                .Include(e => e.WayBillItem)
                    .ThenInclude(bp => bp.BranchProduct)
                        .ThenInclude(mp => mp.MasterProduct)
                .Where(e =>
                e.WayBillItemId != null &&
                e.WayBillItem!.WayBillId == waybillId)
                .ToListAsync();

            if(items == null || items.Count == 0)
            {
                return new List<WayBillDamageItemDto>();
            }

            var damageitems = _mapper.Map<List<WayBillDamageItemDto>>(items);
            return damageitems;
        }
        catch(Exception ex)
        {
            _logger.LogError("Error getting damage items");
            return new List<WayBillDamageItemDto>();
        }
    }

    public async Task<bool> AddDamageWayBillItems(List<WayBillDamageItemDto> dtos)
    {
        if (dtos == null || !dtos.Any())
        {
            _logger.LogWarning("No damage items provided.");
            return false;
        }

        // ✅ Only process valid damage entries
        var validDtos = dtos
            .Where(d =>
                d.WayBillItemId.HasValue &&
                d.DamageQuantity > 0 &&
                d.DamageAmount > 0)
            .ToList();

        if (!validDtos.Any())
        {
            _logger.LogWarning("No valid damage items to insert.");
            return false;
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                /* ---------------------------------
                 * 1️⃣ Insert VALID Damage Items only
                 * --------------------------------- */
                var damageEntities = validDtos.Select(d => new WayBillDamageItem
                {
                    WayBillItemId = d.WayBillItemId!.Value,
                    DamageQuantity = d.DamageQuantity,
                    DamageAmount = d.DamageAmount,
                    TotalDamageCost = d.TotalDamageCost,
                    CreatedAt = PhilippineTime.Now,
                    CreatedBy = GetCurrentUsername()
                }).ToList();

                await _context.WayBillDamageItems.AddRangeAsync(damageEntities);

                /* ---------------------------------
                 * 2️⃣ Update WayBillItems (VALID only)
                 * --------------------------------- */
                var wayBillItemIds = validDtos
                    .Select(d => d.WayBillItemId!.Value)
                    .Distinct()
                    .ToList();

                var wayBillItems = await _context.WayBillItems
                    .Where(w => wayBillItemIds.Contains(w.Id))
                    .ToListAsync();

                foreach (var item in wayBillItems)
                {
                    var totalDamageQty = validDtos
                        .Where(d => d.WayBillItemId == item.Id)
                        .Sum(d => d.DamageQuantity);

                    if (totalDamageQty > item.ActualQuantity)
                        throw new InvalidOperationException(
                            $"Damage quantity exceeds available quantity for WayBillItemId {item.Id}");

                    item.ActualQuantity -= totalDamageQty;

                    // ✅ Recompute total price using remaining quantity
                    item.TotalPrice = item.ActualQuantity * item.ItemPrice;

                    item.UpdatedAt = PhilippineTime.Now;
                    item.UpdatedBy = GetCurrentUsername();
                }

                /* ---------------------------------
                 * 3️⃣ Save + Commit
                 * --------------------------------- */
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "{DamageCount} damage items inserted and {ItemCount} waybill items updated.",
                    damageEntities.Count,
                    wayBillItems.Count);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to insert valid damage items and update waybill items.");
                return false;
            }
        });
    }


    //sync waybill to actual stocks of product

    public async Task<bool> SyncWayBillItemsToBranchProduct(
        List<WayBillItemsDto> items)
    {
        if (items == null || !items.Any())
            return false;

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Load BranchProducts (tracked)
                var branchProductIds = items
                    .Select(x => x.BranchProductId)
                    .Distinct()
                    .ToList();

                var branchProducts = await _context.BranchProducts
                    .Where(bp => branchProductIds.Contains(bp.Id))
                    .ToListAsync();

                // 2️⃣ Load WayBillItems (tracked)
                var wayBillItemIds = items
                    .Select(x => x.Id)
                    .ToList();

                var wayBillItems = await _context.WayBillItems
                    .Where(wi => wayBillItemIds.Contains(wi.Id))
                    .ToListAsync();

                // 3️⃣ Update quantities + mark synced
                foreach (var itemDto in items)
                {
                    var branchProduct = branchProducts
                        .FirstOrDefault(bp => bp.Id == itemDto.BranchProductId);

                    var wayBillItem = wayBillItems
                        .FirstOrDefault(wi => wi.Id == itemDto.Id);

                    if (branchProduct == null || wayBillItem == null)
                        continue;

                    // Prevent double sync
                    if (wayBillItem.IsMergeToSystem)
                        continue;

                    branchProduct.ActualQuantity += itemDto.ActualQuantity;
                    wayBillItem.IsMergeToSystem = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex,
                    "Error syncing WayBill items to BranchProduct");

                throw; // REQUIRED so execution strategy can retry
            }
        });
    }




}

public interface IWayBillService
{
    Task<List<WayBillDto>> GetAllWayBills(BranchOption branch);
    Task<WayBillDto?> GetWayBillById(int wayBill);
    Task<List<WayBillDto>> GetWayBillsBySupplierId(int supplierId);
    Task<List<WayBillItemsDto>> GetAllWayBillItemsByWayBillId(int waybillId);
    Task<bool> CreateWayBill(WayBillDto wayBillDto);
    Task<bool> UpdateWayBill(WayBillDto waybill);
    Task<bool> DeleteWayBill(int wayBillId);

    //waybill damage items

    Task<List<WayBillDamageItemDto>> GetAllDamageItems(int waybillId);
    Task<bool> AddDamageWayBillItems(List<WayBillDamageItemDto> dtos);
    Task<bool> SyncWayBillItemsToBranchProduct(List<WayBillItemsDto> items);
}