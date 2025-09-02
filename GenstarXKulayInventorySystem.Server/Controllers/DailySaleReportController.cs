using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DailySaleReportController : ControllerBase
{
    private readonly IDailySaleReportService _dailySaleReportService;
    public DailySaleReportController(IDailySaleReportService dailySaleReportService)
    {
        _dailySaleReportService = dailySaleReportService;
    }

    [HttpGet("all/{branch}")]
    public async Task<ActionResult<List<DailySaleReportDto>>> GetAllReports(BranchOption branch)
    {
        var reports = await _dailySaleReportService.GetAllDailyReportAsync(branch);
        return Ok(reports);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailySaleReportDto>> GetReportById(int id)
    {
        var report = await _dailySaleReportService.GetDailyReportByIdAsync(id);
        if (report == null)
            return NotFound();
        return Ok(report);
    }

    [HttpPost]
    public async Task<ActionResult<bool>> CreateReport(DailySaleReportDto dto)
    {
        var result = await _dailySaleReportService.AddReportAsync(dto);
        if (!result)
            return BadRequest("Failed to create report.");
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReport(int id, DailySaleReportDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Report ID mismatch.");
        var updatedReport = await _dailySaleReportService.UpdateReportAsync(dto);
        if (!updatedReport)
            return NotFound("Report not found or could not be updated.");
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(int id)
    {
        var deletedReport = await _dailySaleReportService.DeleteReportAsync(id);
        if (!deletedReport)
            return NotFound("Report not found.");
        return Ok();
    }



    //Summary Report routes

    [HttpGet("all/invoice/{date}/{branch}")]
    public async Task<ActionResult<List<DailySaleDto>>> GetAllInvoiceSummary(DateTime date, BranchOption branch)
    {
        var invoiceSales = await _dailySaleReportService.GetAllDailySaleInvoice(date, branch);
        if (invoiceSales == null || invoiceSales.Count == 0)
            return NotFound($"No invoice sales found for {date:yyyy-MM-dd}.");
        return Ok(invoiceSales);
    }

    [HttpGet("all/noninvoice/{date}/{branch}")]
    public async Task<ActionResult<decimal>> GetAllNonInvoiceSummary(DateTime date, BranchOption branch)
    {
        var nonInvoiceSale = await _dailySaleReportService.GetAllDailySaleNonInvoice(date, branch);
        if (nonInvoiceSale == null || nonInvoiceSale.Count == 0)
            return NotFound($"No non-invoice sales found for {date:yyyy-MM-dd}.");
        return Ok(nonInvoiceSale);
    }

}
