
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
 
}
public interface IRequestItemsService
{
    Task<bool> CreateRequestProductItem(RequestProductItemDto model);
}
