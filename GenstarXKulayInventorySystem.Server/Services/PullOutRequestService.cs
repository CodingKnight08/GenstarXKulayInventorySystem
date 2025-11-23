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

    public async Task<bool> CreatePullOutRequest(PullOutRequestDto pullOutRequest)
    {
        try
        {
            var exist = await _context.PullOutRequests.AsNoTracking().AsSplitQuery().Include(p => p.RequestProductItems)
                .Where(e=> !e.IsDeleted && e.RequestProductItems.Count == pullOutRequest.RequestProductItems.Count
                            && e.BranchRequestee == pullOutRequest.BranchRequestee
                            && e.BranchRequestedTo == pullOutRequest.BranchRequestedTo)
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


}
public interface IPullOutRequestService
{
    Task<List<PullOutRequestDto>> GetAllPullOutRequestByBranch(BranchOption branch);
    Task<List<PullOutRequestDto>> GetAllRequesteePullOuts(BranchOption branch);
    Task<bool> CreatePullOutRequest(PullOutRequestDto pullOutRequest);
}