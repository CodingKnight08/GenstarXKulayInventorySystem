using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Threading.Tasks;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Services;

public class PullOutRequestService:IPullOutRequestService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PullOutRequestService> _logger;    
   
    public PullOutRequestService(InventoryDbContext context, IMapper mapper, ILogger<PullOutRequestService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // GetAll who request to you
    public async Task<List<PullOutRequestDto>> GetAllPullOutRequestByBranch(BranchOption branch)
    {
        List<PullOutRequest> pullOuts = await _context.PullOutRequests
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => !p.IsDeleted && p.BranchRequestedTo == branch)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
        if (pullOuts == null || pullOuts.Count == 0)
            return new List<PullOutRequestDto>();
        List<PullOutRequestDto> pullOutRequests = _mapper.Map<List<PullOutRequestDto>>(pullOuts);
        return pullOutRequests;
    }

    // GetAll what you request
    public async Task<List<PullOutRequestDto>> GetAllRequesteePullOuts(BranchOption branch)
    {
        List<PullOutRequest> pullOuts = await _context.PullOutRequests
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => !p.IsDeleted && p.BranchRequestee == branch && !p.Delivered)
            .OrderByDescending (p => p.Id)
            .ToListAsync();

        if(pullOuts == null || pullOuts.Count == 0) 
            return new List<PullOutRequestDto>();

        List<PullOutRequestDto> requests = _mapper.Map<List<PullOutRequestDto>>(pullOuts);
            return requests;
    }


    public async Task<List<PullOutRequestDto>> GetAllRecievedPullOut(BranchOption branch)
    {
        var pullouts = await _context.PullOutRequests.AsNoTracking()
            .AsSplitQuery()
            .Where(e => e.Status == DeliveryStatusOption.Delivered)
            .OrderByDescending(e => e.DateDelivered.HasValue)
            .ThenByDescending(e => e.DateDelivered)
            .ToListAsync();
        if (pullouts is null)
            return new List<PullOutRequestDto>();

        var recievedPullOuts =  _mapper.Map<List<PullOutRequestDto>>(pullouts);
        return recievedPullOuts;
    }
    public async Task<PullOutRequestDto?> GetPullOutRequestById(int id)
    {
        var pullOut = await _context.PullOutRequests
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.RequestProductItems)
            .ThenInclude(rp => rp.MasterProduct)
            .ThenInclude(mp => mp.ProductBrand)
            .Where(p => !p.IsDeleted && p.Id == id)
            .FirstOrDefaultAsync();
        if (pullOut == null)
            return null;
        var pullOutDto = _mapper.Map<PullOutRequestDto>(pullOut);
        return pullOutDto;
    }

    public async Task<bool> CreatePullOutRequest(PullOutRequestDto pullOutRequest)
    {
        try
        {
            var exist = await _context.PullOutRequests
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.RequestProductItems)
                .Where(e => !e.IsDeleted && e.Id == pullOutRequest.Id)
                .FirstOrDefaultAsync();

            if (exist != null)
                return false;

            var newPullOut = _mapper.Map<PullOutRequest>(pullOutRequest);

            var now = PhilippineTime.Now;
            newPullOut.CreatedAt = now;
            newPullOut.DateRequest = pullOutRequest.DateRequest.HasValue
            ? PhilippineTime.ToPH(pullOutRequest.DateRequest.Value).Date
            : null;
            await _context.PullOutRequests.AddAsync(newPullOut);
            int result = await _context.SaveChangesAsync();

            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating pull out request");
            return false;
        }
    }

    public async Task<bool> UpdatePullOutRequest(PullOutRequestDto model)
    {
        try
        {
            var exist = await _context.PullOutRequests
                .FirstOrDefaultAsync(e => !e.IsDeleted && e.Id == model.Id);

            if (exist == null)
                return false;
            exist.Status = model.Status;
            exist.UpdatedAt = PhilippineTime.Now;
            if(model.Status == DeliveryStatusOption.Delivered)
            {
                exist.DateDelivered = exist.DateRequest;
                exist.Delivered = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PullOutRequest");
            return false;
        }
    }

    public async Task<bool> SyncToInventory(
      List<RequestProductItemDto> items)
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
                // 1️⃣ Load Request Items (tracked)
                var requestItemIds = items.Select(x => x.Id).ToList();

                var requestItems = await _context.RequestProductItems
                    .Where(x => requestItemIds.Contains(x.Id))
                    .ToListAsync();

                // 2️⃣ Collect required BranchProducts
                var productIds = items
                    .Where(x => x.MasterProductId.HasValue)
                    .Select(x => x.MasterProductId!.Value)
                    .Distinct()
                    .ToList();

                var branches = items
                    .SelectMany(x => new[] { x.Branch, x.SourceProduct })
                    .Distinct()
                    .ToList();

                var branchProducts = await _context.BranchProducts
                    .Where(bp =>
                        bp.MasterProductId.HasValue &&
                        productIds.Contains(bp.MasterProductId.Value) &&
                        branches.Contains(bp.Branch))
                    .ToListAsync();

                // 3️⃣ Process inventory transfer
                foreach (var dto in items)
                {
                    // 🚫 Skip zero or negative releases
                    if (dto.ReleasedQuantity <= 0)
                        continue;

                    var requestItem = requestItems
                        .FirstOrDefault(x => x.Id == dto.Id);

                    if (requestItem == null || requestItem.IsReceived)
                        continue;

                    if (!dto.MasterProductId.HasValue)
                        continue;

                    var sourceBranchProduct = branchProducts.FirstOrDefault(bp =>
                        bp.MasterProductId == dto.MasterProductId &&
                        bp.Branch == dto.SourceProduct);

                    var requesterBranchProduct = branchProducts.FirstOrDefault(bp =>
                        bp.MasterProductId == dto.MasterProductId &&
                        bp.Branch == dto.Branch);

                    if (sourceBranchProduct == null ||
                        requesterBranchProduct == null)
                    {
                        throw new InvalidOperationException(
                            $"BranchProduct not found for ProductId {dto.MasterProductId}");
                    }

                    // ❌ Prevent negative inventory
                    if (sourceBranchProduct.ActualQuantity < dto.ReleasedQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient stock for product '{dto.MasterProductId}' " +
                            $"in source branch '{dto.SourceProduct}'. " +
                            $"Available: {sourceBranchProduct.ActualQuantity}, " +
                            $"Requested: {dto.ReleasedQuantity}");
                    }

                    // 🔻 Deduct from source
                    sourceBranchProduct.ActualQuantity -= dto.ReleasedQuantity;

                    // 🔺 Add to requester
                    requesterBranchProduct.ActualQuantity += dto.ReleasedQuantity;

                    // ✅ Mark request as received ONLY when quantity > 0
                    requestItem.IsReceived = true;
                    requestItem.DateRecieved = DateTime.UtcNow;
                    requestItem.ReleasedQuantity = dto.ReleasedQuantity;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error syncing inventory transfer");
                throw; // REQUIRED for ExecutionStrategy retry
            }
        });
    }



    public async Task<bool> UpdateRequestItems(List<RequestProductItemDto> items)
    {
        try
        {
            if (items == null || !items.Any())
                return false;

            int pullOutRequestId = items.First().PullOutRequestId;

            // ✅ Load existing items for this request
            var existingItems = await _context.RequestProductItems
                .Where(x => x.PullOutRequestId == pullOutRequestId && !x.IsDeleted)
                .ToListAsync();

            foreach (var dto in items)
            {
                var entity = existingItems.FirstOrDefault(e => e.Id == dto.Id);

                if (entity != null)
                {
                    // ✅ UPDATE
                    _mapper.Map(dto, entity);
                    entity.UpdatedAt = PhilippineTime.Now;
                }
                else
                {
                    // ✅ ADD
                    var newItem = _mapper.Map<RequestProductItem>(dto);
                    newItem.CreatedAt = PhilippineTime.Now;

                    _context.RequestProductItems.Add(newItem);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating RequestProductItems");
            return false;
        }
    }


    public async Task<bool> RecieveRequestItems(List<RequestProductItemDto> items, int pullOutRequestId)
    {
        if (items == null || !items.Any())
            return false;

        try
        {

            // Load existing request items with related branch products
            var existingItems = await _context.RequestProductItems
                .Include(x => x.ProductRequesterBranch)
                .Include(x => x.ProductSourceBranch)
                .Where(x => x.PullOutRequestId == pullOutRequestId && !x.IsDeleted)
                .ToListAsync();

            foreach (var dto in items)
            {
                // Skip items with 0 released quantity
                if (dto.ReleasedQuantity <= 0)
                    continue;

                var entity = existingItems.FirstOrDefault(e => e.Id == dto.Id);

                if (entity != null)
                {
                    // Map updated fields
                    _mapper.Map(dto, entity);
                    entity.UpdatedAt = PhilippineTime.Now;
                    entity.DateRecieved = PhilippineTime.Now;

                    // -----------------------------
                    // Update requester branch product
                    // -----------------------------
                    if (entity.ProductRequesterBranch != null)
                    {
                        entity.ProductRequesterBranch.ActualQuantity += dto.ReleasedQuantity;
                    }

                    // Compute total cost
                    entity.TotalCost = entity.ItemCost * dto.ReleasedQuantity;

                    // -----------------------------
                    // Update source branch product
                    // -----------------------------
                    if (entity.ProductSourceBranch != null)
                    {
                        entity.ProductSourceBranch.ActualQuantity -= dto.ReleasedQuantity;
                        if (entity.ProductSourceBranch.ActualQuantity < 0)
                            entity.ProductSourceBranch.ActualQuantity = 0; // prevent negative stock
                    }
                }
                else
                {
                    // -----------------------------
                    // Add new item
                    // -----------------------------
                    var newItem = _mapper.Map<RequestProductItem>(dto);
                    newItem.CreatedAt = PhilippineTime.Now;

                    // Compute total cost
                    newItem.TotalCost = newItem.ItemCost * newItem.ReleasedQuantity;

                    // Update requester branch
                    if (newItem.ProductRequesterBranch != null)
                        newItem.ProductRequesterBranch.ActualQuantity += newItem.ReleasedQuantity;

                    // Update source branch
                    if (newItem.ProductSourceBranch != null)
                    {
                        newItem.ProductSourceBranch.ActualQuantity -= newItem.ReleasedQuantity;
                        if (newItem.ProductSourceBranch.ActualQuantity < 0)
                            newItem.ProductSourceBranch.ActualQuantity = 0;
                    }

                    _context.RequestProductItems.Add(newItem);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating RequestProductItems");
            return false;
        }
    }


    public async Task<bool> DeletePullOutRequest(int id)
    {
        try
        {
            var exist = await _context.PullOutRequests
                .FirstOrDefaultAsync(e => !e.IsDeleted && e.Id == id);
            if (exist == null)
                return false;
            exist.IsDeleted = true;
            exist.UpdatedAt = PhilippineTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting PullOutRequest");
            return false;
        }
    }


}
public interface IPullOutRequestService
{
    Task<List<PullOutRequestDto>> GetAllPullOutRequestByBranch(BranchOption branch);
    Task<List<PullOutRequestDto>> GetAllRequesteePullOuts(BranchOption branch);
    Task<List<PullOutRequestDto>> GetAllRecievedPullOut(BranchOption branch);
    Task<PullOutRequestDto?> GetPullOutRequestById(int id);
    Task<bool> CreatePullOutRequest(PullOutRequestDto pullOutRequest);
    Task<bool> UpdatePullOutRequest(PullOutRequestDto model);
    Task<bool> UpdateRequestItems(List<RequestProductItemDto> items);
    Task<bool> RecieveRequestItems(List<RequestProductItemDto> items, int pullOutRequestId);
    Task<bool> SyncToInventory(List<RequestProductItemDto> items);
    Task<bool> DeletePullOutRequest(int id);
}