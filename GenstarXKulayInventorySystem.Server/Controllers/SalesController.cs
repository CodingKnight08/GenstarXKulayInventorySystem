using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
//[Authorize]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _saleService;
    private readonly ILogger<SalesController> _logger;

    public SalesController(ISalesService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllDailySales()
    {
        var sales = await _saleService.GetAllDailySalesAsync();
        return Ok(sales);
    }

    [HttpGet("paged/by/{branch}/{date}")]
    public async Task<ActionResult<DailySalePageResultDto<DailySaleDto>>> GetAllDailySalesByBranch(
    BranchOption branch,
    DateTime date,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 10)
    {
        try
        {
            var dailySales = await _saleService.GetAllDailySaleByBranch(branch, date);
            if (dailySales == null || !dailySales.Any())
                return new DailySalePageResultDto<DailySaleDto> { Sales = new List<DailySaleDto>(), TotalCount = 0 };
            var total = dailySales.Count;
            var pageItems = dailySales.Skip(skip).Take(take).ToList();
            return Ok(new DailySalePageResultDto<DailySaleDto>
            {
                Sales = pageItems,
                TotalCount = total
            });
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Error retrieving daily sales: {ex.Message}");
        }
    }
    [HttpGet("all/{branch}/{date}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetDailySalesByBranch(
        BranchOption branch,
        DateTime date)
    {
        var sales = await _saleService.GetAllDailySaleByBranch(branch, date);

        return Ok(sales ?? new List<DailySaleDto>());
    }


    [HttpGet("all/range/{range}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetDailySalesByRange(DateRangeOption range)
    {
        var sales = await _saleService.GetAllDailySaleByDaysAsync(range);

        if (sales == null || sales.Count == 0)
            return NotFound("No sales found for the given date range.");

        return Ok(sales);
    }

    [HttpGet("by-date/{date}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetDailySalesByDate(DateTime date)
    {
        var sales = await _saleService.GetAllDailySalesByDaySetAsync(date);

        if (sales == null || sales.Count == 0)
            return NotFound($"No sales found for {date:yyyy-MM-dd}.");

        return Ok(sales);
    }

    [HttpGet("all/paid/{date}/{branch}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllPaidSalesByDate(DateTime date,BranchOption branch)
    {
        var sales = await _saleService.GetAllDailySalesPaidAsync(date,branch);
        if (sales == null || sales.Count == 0)
            return NotFound($"No paid sales found for {date:yyyy-MM-dd}.");
        return Ok(sales);
    }

    [HttpGet("all/unpaid/{date}/{branch}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllUnpaidSalesByDate(DateTime date, BranchOption branch)
    {
        var sales = await _saleService.GetAllDailySalesUnpaidAsync(date,branch);
        if (sales == null || sales.Count == 0)
            return NotFound($"No unpaid sales found for {date:yyyy-MM-dd}.");
        return Ok(sales);
    }

    [HttpGet("all/collection/{date}/{branch}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllCollectionSalesByDate(DateTime date, BranchOption branch)
    {
        var sales = await _saleService.GetAllCollectionAsync(date,branch);
        if (sales == null || sales.Count == 0)
            return NotFound($"No collection sales found for {date:yyyy-MM-dd}.");
        return Ok(sales);
    }


    [HttpGet("all/invoices/{date}")]
    public async Task<ActionResult<string>> GetAllSalesWithInvoicesByDate(DateTime date)
    {
        var totalInvoiceSale = await _saleService.GetAllDailyInvoiceAsync(date);

        if (totalInvoiceSale <= 0)
            return NotFound($"No sales with invoices found for {date:yyyy-MM-dd}.");

        return Ok(totalInvoiceSale.ToString("F2") );
    }

    [HttpGet("all/nonvoices/{date}")]
    public async Task<ActionResult<string>> GetAllSalesNonvoiceByDate(DateTime date)
    {
        var totalNonInvoiceSale = await _saleService.GetAllDailyNonVoiceAsync(date);
        if (totalNonInvoiceSale <= 0)
            return NotFound($"No sales without invoices found for {date:yyyy-MM-dd}.");
        return Ok(totalNonInvoiceSale.ToString("F2"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailySaleDto?>> GetDailySaleById(int id)
    {
        var sale = await _saleService.GetDailySaleByIdAsync(id);
        if (sale == null)
        {
            return NotFound();
        }

        return Ok(sale);
    }


    [HttpPost]
    public async Task<IActionResult> CreateSale(DailySaleDto dto)
    {
        try
        {
            var result = await _saleService.AddAsync(dto);
            if(!result)
                return BadRequest("Sale already exist");
            return Ok(result);
        }
        catch (Exception ex) {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSale(int id, DailySaleDto dto)
    {
        if(id != dto.Id)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var result = await _saleService.UpdateAsync(dto);
            if (!result)
                return NotFound("Sale not found to be updated or mismatch");
            return Ok(result);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

    }
    [HttpPut("setpaid/{id}")]
    public async Task<IActionResult> SetPaid(int id)
    {
        if(id == 0)
        {
            return BadRequest("Id invalid");
        }
        try
        {
            var result = await _saleService.SetPaid(id);
            if (!result)
                return NotFound("Sale setting paid failed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSale(int id)
    {
        try
        {
            var result = await _saleService.DeleteSaleAsync(id);
            if (!result)
                return NotFound("Sale info not found");
            return Ok(result);
        }
        catch (Exception ex) 
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpPost("returnitems")]
    public async Task<IActionResult> AddReturnSales([FromBody] AddReturnSalesRequest request)
    {
        if (request == null || request.ReturnItems == null || !request.ReturnItems.Any())
            return BadRequest("No return items provided.");

        try
        {
            var result = await _saleService.AddReturnSales(
                request.ReturnItems,
                request.DailySaleId);

            if (!result)
                return BadRequest("Failed to add return sales.");

            return Ok(new
            {
                success = true,
                count = request.ReturnItems.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding return sales");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("return/{id:int}")]
    public async Task<IActionResult> UpdateSaleTotal(
    int id,
    [FromQuery] decimal returnTotal)
    {
        if (returnTotal <= 0)
            return BadRequest("Invalid return total.");

        try
        {
            var success = await _saleService.UpdateSalesTotal(id, returnTotal);

            if (!success)
                return NotFound("Sale not found or update failed.");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sale total for SaleId {SaleId}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }



}
