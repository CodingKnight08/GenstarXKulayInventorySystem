using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
            .Where(p => !p.IsDeleted && p.BranchRequestee == branch)
            .OrderByDescending (p => p.Id)
            .ToListAsync();

        if(pullOuts == null || pullOuts.Count == 0) 
            return new List<PullOutRequestDto>();

        List<PullOutRequestDto> requests = _mapper.Map<List<PullOutRequestDto>>(pullOuts);
            return requests;
    }
    public async Task<PullOutRequestDto?> GetPullOutRequestById(int id)
    {
        var pullOut = await _context.PullOutRequests
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.RequestProductItems)
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
            var exist = await _context.PullOutRequests.AsNoTracking().AsSplitQuery().Include(p => p.RequestProductItems)
                .Where(e=> !e.IsDeleted && e.Id == pullOutRequest.Id)
                .FirstOrDefaultAsync();

            if(exist !=null)
                return false;

            var newPullOut = _mapper.Map<PullOutRequest>(pullOutRequest);
            newPullOut.CreatedAt = PhilippineTime.Now;
            await _context.PullOutRequests.AddAsync(newPullOut);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex) {
            _logger.LogError(ex.Message);
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

            _mapper.Map(model, exist);
            exist.UpdatedAt = PhilippineTime.Now;


            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating PullOutRequest");
            return false;
        }
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

            // =============================
            // DELETE removed items
            // =============================
            var toRemove = existingItems
                .Where(e => !items.Any(i => i.Id == e.Id))
                .ToList();

            foreach (var item in toRemove)
                _context.RequestProductItems.Remove(item);
            // or soft-delete:
            // item.IsDeleted = true;

            // =============================
            // ADD / UPDATE items
            // =============================
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





}
public interface IPullOutRequestService
{
    Task<List<PullOutRequestDto>> GetAllPullOutRequestByBranch(BranchOption branch);
    Task<List<PullOutRequestDto>> GetAllRequesteePullOuts(BranchOption branch);
    Task<PullOutRequestDto?> GetPullOutRequestById(int id);
    Task<bool> CreatePullOutRequest(PullOutRequestDto pullOutRequest);
    Task<bool> UpdatePullOutRequest(PullOutRequestDto model);
    Task<bool> UpdateRequestItems(List<RequestProductItemDto> items);
}