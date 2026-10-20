using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatementReportController : ControllerBase
{
    private readonly IStatementReportService _statementReportService; 
    public StatementReportController(IStatementReportService statementReportService)
    {
        _statementReportService = statementReportService;
    }


    [HttpGet("clients/{branch}")]
    public async Task<ActionResult<List<ClientDto>>> GetAllClientWithCharges([FromRoute] BranchOption branch)
    {
        var clients = await _statementReportService.GetAllClientsWithChargedSales(branch);
        return Ok(clients);
    }

    [HttpGet("{clientId}")]
    public async Task<ActionResult<ClientDto>> GetClientChargeSales([FromRoute] int clientId)
    {
        var client = await _statementReportService.GetClientChargeSales(clientId);
        if (client == null)
        {
            return NotFound();
        }
        return Ok(client);
    }
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateStatementPdf(
    [FromBody] StatementOfAccountDataDto dto,
    [FromQuery] int clientId,
    [FromQuery] int month)
    {
        try
        {
            var pdfBytes =
                await _statementReportService.GenerateStatementPdfAsync(dto);

            return File(
                pdfBytes,
                "application/pdf",
                $"Statement_{clientId}_{month}_{DateTime.Now.Year}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("monthly-charges/{clientId}/{month}/{year}")]
    public async Task<IActionResult> GetMonthlyCharges(int clientId, int month, int year)
    {
        var data = await _statementReportService.GetChargeSalesForThatMonth(clientId, month, year);

        if (data == null)
            return NotFound();

        return Ok(data);
    }

    [HttpPut("{clientId}/remaining-balance")]
    public async Task<IActionResult> UpdateRemainingBalance(int clientId, [FromBody] ClientDto dto)
    {
        if (dto == null)
            return BadRequest("Client data is required.");

        var result = await _statementReportService.UpdateChargeSale(clientId, dto);

        if (!result)
            return NotFound($"Client with ID {clientId} not found.");

        return Ok(new { message = "Remaining balance updated successfully." });
    }

}
