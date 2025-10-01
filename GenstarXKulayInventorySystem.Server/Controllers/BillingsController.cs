using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class BillingsController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingsController> _logger;
    public BillingsController(IBillingService billingService, ILogger<BillingsController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }
    //Operational Billings
    [HttpGet("all/others")]
    public async Task<ActionResult<List<BillingDto>>> GetAllBillings()
    {
        try
        {


            var billings = await _billingService.GetAllBillingAsync();
            return Ok(billings);
        }
        catch (Exception ex)
        {
            // log the full error
            _logger.LogError(ex, "Error while getting billings");
            return StatusCode(500, ex.Message); 
        }
    }

    [HttpGet("all/expenses/{date}/{branch}")]
    public async Task<ActionResult<List<BillingDto>>> GetAllDailyExpenses(DateTime date, BillingBranch branch)
    {
        var billings = await _billingService.GetAllExpensesBillingPerDay(date, branch);
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


    [HttpGet("purchase-orders/all")]
    public async Task<ActionResult<List<PurchaseOrderBillingDto>>> GetAllPurchaseOrderBillings()
    {
        var billings = await _billingService.GetAllPurchaseOrderBillingAsync();
        return Ok(billings);
    }

    [HttpGet("purchase-orders/{id:int}")]
    public async Task<ActionResult<PurchaseOrderBillingDto>> GetPurchaseOrderBillingById(int id)
    {
        var billing = await _billingService.GetPurchaseOrderBillingById(id);
        if (billing == null)
            return NotFound("Purchase Order Billing not found.");
        return Ok(billing);
    }
    [HttpPost("purchase-orders")]
    public async Task<IActionResult> CreatePurchaseOrderBilling(PurchaseOrderBillingDto purchaseOrderBillingDto)
    {
        try
        {
            var result = await _billingService.AddPurchaseOrderBilling(purchaseOrderBillingDto);
            if (!result)
                return BadRequest("Purchase Order Billing already exists.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("purchase-orders/{id:int}")]
    public async Task<IActionResult> UpdatePurchaseOrderBilling(int id, PurchaseOrderBillingDto purchaseOrderBillingDto)
    {
        try
        {
            if (id != purchaseOrderBillingDto.Id)
                return BadRequest("Purchase Order Billing ID mismatch.");
            var result = await _billingService.UpdatePurchaseOrderBilling(purchaseOrderBillingDto);
            if (!result)
                return NotFound("Purchase Order Billing not found or already deleted.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("purchase-orders/delete/{id:int}")]
    public async Task<IActionResult> DeletePurchaseOrderBilling(int id)
    {
        try
        {
            var result = await _billingService.DeletePurchaseOrderBilling(id);
            if (!result)
                return NotFound("Purchase Order Billing not found or already deleted.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
