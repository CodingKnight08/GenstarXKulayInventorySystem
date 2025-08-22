using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SalesItemController : ControllerBase
{
   private readonly ISaleItemService _saleItemService;
   
    public SalesItemController(ISaleItemService saleItemService)
    {
        _saleItemService = saleItemService;
    }

    [HttpGet("all/{dailySaleId:int}")]
    public async Task<ActionResult<List<SaleItemDto>>> GetAllSaleItemsByDailySale(int dailySaleId)
    {
        var saleItems = await _saleItemService.GetAllSaleItemsAsync(dailySaleId);
        if (saleItems == null || saleItems.Count == 0) { 
         return NotFound("No sale items found");
        }
        return Ok(saleItems);
    }

    [HttpGet("all/undeducted")]
    public async Task<ActionResult<List<SaleItemDto>>> GetAllUndeductedItems()
    {
        var saleItems = await _saleItemService.GetAllUndeductedItemsAsync();
        if(saleItems == null || saleItems.Count == 0)
        {
            return NotFound("No sale items to be processed found");
        }
        return Ok(saleItems);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleItemDto>> GetSaleItem(int id)
    {
        var saleItem = await _saleItemService.GetSaleItemById(id);
        if (saleItem == null) { 
            return NotFound();
        }
        return Ok(saleItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSaleItem(SaleItemDto saleItemDto)
    {
        try
        {
            var result = await _saleItemService.AddSaleItemAsync(saleItemDto);
            if (!result) {
                return BadRequest("Sale item already exist");
            }
            return Ok(result);
        }
        catch (Exception ex) {
            return StatusCode(500, ex.Message);
        
        } 
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSaleItem(int id, SaleItemDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest("ID mismatch");
        }
        try
        {
            var result = await _saleItemService.UpdateSaleItemAsync(dto);
            if (!result) {
                return NotFound("Sale item not found to be updated or mismatch");
            }
            return Ok(result);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSaleItem(int id)
    {
        try
        {
            var result = await _saleItemService.DeleteSaleItemAsync(id);
            if (!result)
            {
                return NotFound("Sale Item info not found");
            }
            return Ok(result);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
