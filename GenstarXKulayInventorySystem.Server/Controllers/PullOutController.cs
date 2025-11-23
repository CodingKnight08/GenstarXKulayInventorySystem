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
    public PullOutController(IPullOutRequestService service)
    {
        _service = service;
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
}
