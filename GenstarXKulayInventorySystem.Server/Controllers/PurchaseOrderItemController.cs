using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PurchaseOrderItemController : ControllerBase
{
    private readonly IPurchaseOrderItemService _service;
    public PurchaseOrderItemController(IPurchaseOrderItemService service)
    {
        _service = service;
    }

    [HttpGet("{id:int}/items/unreceived")]
    public async Task<ActionResult<List<PurchaseOrderItemDto>>> GetAllUnreceivedItems(int id)
    {
        var items = await _service.GetAllUnrecieveItemsAsync(id);
        return Ok(items);
    }

    [HttpGet("{id:int}/items/received")]
    public async Task<ActionResult<List<PurchaseOrderItemDto>>> GetAllReceivedItems(int id)
    {
        var items = await _service.GetAllRecieveItemsAsync(id);
        return Ok(items);
    }

    [HttpGet("all/recieved/purchaseorderitems")]
    public async Task<ActionResult<List<PurchaseOrderItemDto>>> GetPurchaseOrdersForBilling()
    {
        var items = await _service.GetAllUnrecieveItemsByDate();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderItemDto?>> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PurchaseOrderItemDto dto)
    {
        try
        {
            var result = await _service.AddAsync(dto);
            if (!result)
                return BadRequest("Purchase order item already exists.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PurchaseOrderItemDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");
        try
        {
            var result = await _service.UpdateAsync(dto);
            if (!result)
                return NotFound("Purchase order item not found or already processed.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("mark-as-received")]
    public async Task<IActionResult> MarkAsReceived(List<PurchaseOrderDto> dtos)
    {
        try
        {
            var result = await _service.MarkAsRecievedAsync(dtos);
            if (!result)
                return BadRequest("No items to mark as received or already processed.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpPut("update-items")]
    public async Task<IActionResult> UpdateItems(List<PurchaseOrderItemDto> dtos)
    {
        try
        {
            var result = await _service.UpdateItemsAsync(dtos);
            if (!result)
                return BadRequest("No items to update or already processed.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound("Purchase order item not found or already deleted.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

}
