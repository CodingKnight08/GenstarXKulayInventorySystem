using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PurchaseOrderController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrderController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<PurchaseOrderDto>>> GetAll()
    {
        var purchaseOrders = await _service.GetAllAsync();
        return Ok(purchaseOrders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderDto?>> GetById(int id)
    {
        var purchaseOrder = await _service.GetByIdAsync(id);
        if (purchaseOrder == null)
            return NotFound();
        return Ok(purchaseOrder);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PurchaseOrderDto dto)
    {
        try
        {
            var result = await _service.AddAsync(dto);
            if (!result)
                return BadRequest("Purchase order already exists.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PurchaseOrderDto dto)
    {
        if (id != dto.Id)
            return BadRequest("ID mismatch");
        try
        {
            var result = await _service.UpdateAsync(dto);
            if (!result)
                return NotFound("Purchase order not found or already processed.");
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
                return NotFound("Purchase order not found or already processed.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
