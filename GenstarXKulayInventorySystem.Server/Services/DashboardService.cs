using AutoMapper;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GenstarXKulayInventorySystem.Server.Services;

public class DashboardService : IDashboardService
{
    private readonly InventoryDbContext _dbContext;
    private readonly ILogger<DashboardService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public DashboardService(InventoryDbContext dbContext, ILogger<DashboardService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }


    public async Task<DashBoardDto> GetDailySalesExpenses()
    {
        var today = DateTime.Today;
        var totalProductCosts = await _dbContext.DailySales
            .AsNoTracking().AsSplitQuery()
            .Include(ds=> ds.SaleItems)
            .Where(ds => ds.DateOfSales.Date == today && !ds.IsDeleted )
            .SumAsync(ds => ds.SaleItems.Sum(si => si.CostPrice * si.Quantity));
        var totalNetSales = await _dbContext.DailySales
            .AsNoTracking().AsSplitQuery()
            .Include(ds => ds.SaleItems)
            .Where(ds => ds.DateOfSales.Date == today && !ds.IsDeleted)
            .SumAsync(ds => ds.SaleItems.Sum(si => si.ItemPrice * si.Quantity));
       
        return new DashBoardDto
        {
            TotalProductCosts = totalProductCosts,
            TotalNetSales = totalNetSales
        };
    }

}

public interface IDashboardService
{
    
}