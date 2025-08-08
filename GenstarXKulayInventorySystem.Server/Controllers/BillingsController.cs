using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BillingsController : ControllerBase
{
    private readonly IBillingService _billingService;
    public BillingsController(IBillingService billingService)
    {
        _billingService = billingService;
    }
    //Operational Billings
    [HttpGet("all/others")]
    public async Task<ActionResult<List<BillingDto>>> GetAllBillings()
    {
        var billings = await _billingService.GetAllNotPurchaseOrderBillings();
        return Ok(billings);
    }
    [HttpGet("operational/{id:int}")]
    public async Task<ActionResult<BillingDto>> GetBillingById(int id)
    {
        var billing = await _billingService.GetBillingById(id);
        if (billing == null)
            return NotFound("Billing not found.");
        return Ok(billing);
    }

    [HttpPost("operational")]
    public async Task<IActionResult> CreateBilling(BillingDto billingDto)
    {
        try
        {
            var result = await _billingService.AddBillingAsync(billingDto);
            if (!result)
                return BadRequest("Billing already exists.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpPut("operational/{id:int}")]

    public async Task<IActionResult> UpdateBilling(int id, BillingDto billingDto)
    {
        try
        {
            if (id != billingDto.Id)
                return BadRequest("Billing ID mismatch.");
            var result = await _billingService.UpdateBillingAsync(billingDto);
            if (!result)
                return NotFound("Billing not found or already deleted.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("operational/delete/{id:int}")]
    public async Task<IActionResult> DeleteBilling(int id)
    {
        try
        {
            var result = await _billingService.DeleteBillingAsync(id);
            if (!result)
                return NotFound("Billing not found or already deleted.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    //Puchase Order Billings
}
