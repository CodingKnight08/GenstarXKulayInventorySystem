using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _saleService;

    public SalesController(ISalesService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllDailySales()
    {
        var sales = await _saleService.GetAllDailySalesAsync();
        return Ok(sales);
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

    [HttpGet("all/paid/{date}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllPaidSalesByDate(DateTime date)
    {
        var sales = await _saleService.GetAllDailySalesPaidAsync(date);
        if (sales == null || sales.Count == 0)
            return NotFound($"No paid sales found for {date:yyyy-MM-dd}.");
        return Ok(sales);
    }

    [HttpGet("all/unpaid/{date}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllUnpaidSalesByDate(DateTime date)
    {
        var sales = await _saleService.GetAllDailySalesUnpaidAsync(date);
        if (sales == null || sales.Count == 0)
            return NotFound($"No unpaid sales found for {date:yyyy-MM-dd}.");
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSale(int id)
    {
        try
        {
            var result = await _saleService.DeleteSaleASync(id);
            if (!result)
                return NotFound("Sale info not found");
            return Ok(result);
        }
        catch (Exception ex) 
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
