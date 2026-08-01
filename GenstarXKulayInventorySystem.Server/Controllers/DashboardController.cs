using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }
    [HttpGet("daily/{branch}")]
    public async Task<ActionResult<DashBoardDto>> GetDailyDashboardData(
        BranchOption branch,
        [FromQuery] DateTime? date)
    {
        try
        {
            var dashboardData = await _dashboardService.GetDailySalesNetAndCOGS(
                branch,
                date ?? DateTime.Today);

            return Ok(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting daily dashboard data");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("daily-expenses/{branch}")]
    public async Task<ActionResult<decimal>> GetDailyExpenses(
        BranchOption branch,
        [FromQuery] DateTime? date)
    {
        try
        {
            var result = await _dashboardService.GetDailyExpenses(
                branch,
                date ?? DateTime.Today);

            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting daily expenses");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("weekly-chart/{branch}")]
    public async Task<ActionResult<List<DashboardChartDto>>> GetWeeklyChart(
     BranchOption branch,
     [FromQuery] DateTime? date)
    {
        try
        {
            var result = await _dashboardService.GetWeeklyExpenseProfitChart(
                branch,
                date ?? DateTime.UtcNow);

            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting weekly chart data");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("monthly-chart/{branch}")]
    public async Task<ActionResult<List<DashboardChartDto>>> GetMonthlyChart(
     BranchOption branch,
     [FromQuery] DateTime? date)
    {
        try
        {
            var result = await _dashboardService.GetMonthlyExpenseProfitChart(
                branch,
                date ?? DateTime.UtcNow);

            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting monthly dashboard chart");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("yearly-chart/{branch}")]
    public async Task<ActionResult<List<DashboardChartDto>>> GetYearlyChart(BranchOption branch)
    {
        try
        {
            var result = await _dashboardService.GetYearlyExpenseProfitChart(branch);

            return Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting yearly dashboard chart");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("weekly-top-products/{branch}")]
    public async Task<ActionResult<List<TopSaleItemDto>>> GetWeeklyTopProducts(
    BranchOption branch,
    [FromQuery] DateTime? date)
    {
        try
        {
            var result = await _dashboardService.GetWeeklyTopProducts(
                branch,
                date ?? DateTime.UtcNow);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed loading top products");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("monthly-top-products/{branch}")]
    public async Task<ActionResult<List<TopSaleItemDto>>> GetMonthlyTopProducts(
    BranchOption branch,
    [FromQuery] DateTime? date)
    {
        try
        {
            var result = await _dashboardService.GetMonthlyTopProducts(
                branch,
                date ?? DateTime.UtcNow);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed loading monthly top products");
            return StatusCode(500, ex.Message);
        }
    }
}
