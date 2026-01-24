using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PullOutController : ControllerBase
{
    private readonly IPullOutRequestService _service;
    private readonly ILogger<PullOutController> _logger;
    private readonly IRequestItemsService _itemsService;
    public PullOutController(IPullOutRequestService service, ILogger<PullOutController> logger, IRequestItemsService itemsService)
    {
        _service = service;
        _logger = logger;
        _itemsService = itemsService;
    }


    [HttpGet("all/requests/{branch}")]
    public async Task<ActionResult<List<PullOutRequestDto>>> GetAllRequests(BranchOption branch)
    {
        var requests = await _service.GetAllRequesteePullOuts(branch);
        return Ok(requests);
    }

    [HttpGet("all/requester/{branch}")]
    public async Task<ActionResult<List<PullOutRequestDto>>> GetAllRequester(BranchOption branch)
    {
        var requesters = await _service.GetAllPullOutRequestByBranch(branch);
        return Ok(requesters);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PullOutRequestDto>> GetRequestById(int id)
    {
        var request = await _service.GetPullOutRequestById(id);
        if (request == null)
            return NotFound("Request not found");
        return Ok(request);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePullOutRequest(PullOutRequestDto dto)
    {
        try
        {
            var result = await _service.CreatePullOutRequest(dto);
            if (!result)
                return BadRequest("Creation failed");
            return Ok(result);

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePullOutRequest(PullOutRequestDto dto)
    {
        try
        {
            var result = await _service.UpdatePullOutRequest(dto);
            if (!result)
                return BadRequest("Update failed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut("recieved/items/{pullOutRequestId}")]
    public async Task<IActionResult> RecievePullOutRequestItems(
     [FromBody] List<RequestProductItemDto> dto,
     int pullOutRequestId)
    {
        if (dto == null || !dto.Any())
            return BadRequest("No items provided.");

        try
        {
            // Ensure DTOs have the correct PullOutRequestId
            dto.ForEach(d => d.PullOutRequestId = pullOutRequestId);

            var result = await _service.RecieveRequestItems(dto, pullOutRequestId);

            if (!result)
                return BadRequest("Update failed.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating pull-out request items: {ex.Message}");
        }
    }

    [HttpPut("items")]
    public async Task<IActionResult> UpdatePullOutRequestItems(List<RequestProductItemDto> dto)
    { try 
        { var result = await _service.UpdateRequestItems(dto); 
            if (!result) return BadRequest("Update failed"); return Ok(result);
        } 
        catch
        (Exception ex) 
        { return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateRequestItems(RequestProductItemDto dto)
    {
        try
        {
            var result = await _itemsService.UpdateItemStatus(dto);
            if (!result)
                return BadRequest("Update failed");
            return Ok(result);
        }
        catch(Exception ex)
        {
           return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("sync-request-items")]
    public async Task<IActionResult> SyncRequestItems([FromBody] List<RequestProductItemDto> items)
    {
        if (items == null || !items.Any())
            return BadRequest("No items provided.");

        try
        {
            var result = await _service.SyncToInventory(items);

            if (result)
                return Ok(new { message = "Inventory synced successfully." });

            return StatusCode(500, new { message = "Failed to sync inventory." });
        }
        catch (InvalidOperationException ex)
        {
            // Business rule exception, e.g., negative stock
            _logger.LogWarning(ex, "Sync failed due to business rule.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error syncing inventory.");
            return StatusCode(500, new { message = "Unexpected error occurred." });
        }
    }

}
