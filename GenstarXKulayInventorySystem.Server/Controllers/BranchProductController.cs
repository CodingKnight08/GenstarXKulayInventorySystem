using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchProductController : ControllerBase
{
    private readonly IBranchProductService _service;
    private readonly ILogger<BranchProductController> _logger;
    public BranchProductController(IBranchProductService service, ILogger<BranchProductController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPut("update-prices")]
    public async Task<IActionResult> UpdatePrices([FromBody] List<BranchProductPrice> items)
    {
        if (items == null || !items.Any())
            return BadRequest("No data received.");

        try
        {
            bool result = await _service.UpdateBranchProductPrices(items);
            if (result)
            {
                return Ok($"Updated {items.Count} branch products.");
            }
            else
            {
                return StatusCode(500, "Failed to update branch product prices.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch product prices");
            return StatusCode(500, ex.Message);
        }
    }

}
