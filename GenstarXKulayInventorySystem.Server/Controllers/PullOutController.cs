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
    [HttpPut("items")]
    public async Task<IActionResult> UpdatePullOutRequestItems(List<RequestProductItemDto> dto)
    {
        try
        {
            var result = await _service.UpdateRequestItems(dto);
            if (!result)
                return BadRequest("Update failed");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
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
}
