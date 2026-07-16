
using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Services;

public class RequestItemsService:IRequestItemsService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RequestItemsService> _logger;

    public RequestItemsService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<RequestItemsService> logger)
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

    public async Task<List<RequestProductItemDto>> GetAllRequestedProducts(BranchOption branch)
    {
        List<RequestProductItem> requests = await _context.RequestProductItems
            .AsNoTracking().AsSplitQuery().Where(e => e.Branch == branch && !e.IsDeleted).ToListAsync();
        if(requests == null || requests.Count == 0)
        {
            return new List<RequestProductItemDto>();
        }
        List<RequestProductItemDto> requestDto = _mapper.Map<List<RequestProductItemDto>>(requests);
        return requestDto;
                                            
    }


    public async Task<bool> CreateRequestProductItem(RequestProductItemDto model)
    {
        try
        {
            var existingReq = await _context.RequestProductItems.AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(e => e.Id == model.Id && !e.IsDeleted);
            if (existingReq != null)
            {
                return false;
            }
            var request = _mapper.Map<RequestProductItem>(model);
            request.CreatedAt = PhilippineTime.Now;
            request.CreatedBy = GetCurrentUsername();
            _ = await _context.RequestProductItems.AddAsync(request);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }

        catch (Exception ex)
        {
            _logger.LogError(ex.Message, "Error in creating request product");
            return false;
        }
    }
    public async Task<bool> UpdateItemStatus(RequestProductItemDto dto)
    {
        if (dto == null || dto.Id <= 0)
            return false;

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existReq = await _context.RequestProductItems
                    .FirstOrDefaultAsync(e => e.Id == dto.Id && !e.IsDeleted);

                if (existReq == null)
                    return false;

                // 🔒 Prevent double processing
                bool wasAlreadyReceived = existReq.IsReceived;

                // ✅ Update request item
                existReq.IsReceived = dto.IsReceived;
                existReq.ReleasedQuantity = dto.ReleasedQuantity;
                existReq.UpdatedAt = PhilippineTime.Now;
                existReq.UpdatedBy = GetCurrentUsername();
                existReq.TotalCost = existReq.ItemCost * dto.ReleasedQuantity;

                if (dto.IsReceived && !wasAlreadyReceived)
                    existReq.DateRecieved = PhilippineTime.Now;

                // ✅ Update inventories ONLY once
                if (dto.IsReceived &&
                    !wasAlreadyReceived &&
                    dto.ReleasedQuantity > 0)
                {
                    // 🔻 SOURCE BRANCH
                    if (!dto.ProductSourceBranchId.HasValue)
                        throw new Exception("Source branch product ID is missing");

                    var sourceProduct = await _context.BranchProducts
                        .FirstOrDefaultAsync(bp => bp.Id == dto.ProductSourceBranchId.Value);

                    if (sourceProduct == null)
                        throw new Exception("Source branch product not found");

                    sourceProduct.ActualQuantity -= dto.ReleasedQuantity;

                    if (sourceProduct.ActualQuantity < 0)
                        throw new Exception("Source product stock cannot go below zero");

                    // 🔺 REQUESTER BRANCH
                    if (!dto.ProductRequesterBranchId.HasValue)
                        throw new Exception("Requester branch product ID is missing");

                    var requesterProduct = await _context.BranchProducts
                        .FirstOrDefaultAsync(bp => bp.Id == dto.ProductRequesterBranchId.Value);

                    if (requesterProduct == null)
                        throw new Exception("Requester branch product not found");

                    requesterProduct.ActualQuantity += dto.ReleasedQuantity;
                }

                var result = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating request item status");
                return false;
            }
        });
    }


}
public interface IRequestItemsService
{
    Task<bool> CreateRequestProductItem(RequestProductItemDto model);
    Task<bool> UpdateItemStatus(RequestProductItemDto dto);
}
