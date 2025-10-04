using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OperationsProviderController : ControllerBase
{
    private readonly IOperationsProviderService _operationsProviderService;
    public OperationsProviderController(IOperationsProviderService operationsProviderService)
    {
        _operationsProviderService = operationsProviderService;
    }

    [HttpGet("all/{branch}")]
    public async Task<ActionResult<List<OperationsProviderDto>>> GetAllOperationsProvider(BranchOption branch)
    {
        var operationsProviders = await _operationsProviderService.GetAllOperationsProvider(branch);
        return Ok(operationsProviders);
    }

    [HttpGet("paged/by/{branch}")]
    public async Task<ActionResult<OperationsProviderPageResultDto<OperationsProviderDto>>> GetPagedOperationsProviders(BranchOption branch, [FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        var providers = await _operationsProviderService.GetAllOperationsProvider(branch);
        if(providers == null || !providers.Any())
            return new OperationsProviderPageResultDto<OperationsProviderDto> { OperationsProviders = new(), TotalCount = 0 };
        var total = providers.Count;
        var pagedProviders = providers.Skip(skip).Take(take).ToList();
        var pagedResult = new OperationsProviderPageResultDto<OperationsProviderDto>
        {
            OperationsProviders = pagedProviders,
            TotalCount = total
        };
        return Ok(pagedResult);
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OperationsProviderDto>> GetOperationsProviderById(int id)
    {
        var operationsProvider = await _operationsProviderService.GetOperationsProviderById(id);
        if (operationsProvider == null)
            return NotFound("Operations Provider not found.");
        return Ok(operationsProvider);
    }
    [HttpPost]
    public async Task<IActionResult> CreateOperationsProvider(OperationsProviderDto operationsProviderDto)
    {
        try
        {
            var result = await _operationsProviderService.AddOperationsProvider(operationsProviderDto);
            if (result == 0)
                return BadRequest("Operations Provider already exists.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOperationsProvider(int id, OperationsProviderDto operationsProviderDto)
    {
        try
        {
            if (id != operationsProviderDto.Id)
                return BadRequest("Operations Provider ID mismatch.");
            var result = await _operationsProviderService.UpdateOperationsProvider(operationsProviderDto);
            if (!result)
                return NotFound("Operations Provider not found.");

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOperationsProvider(int id)
    {
        try
        {
            var result = await _operationsProviderService.DeleteOperationsProvider(id);
            if (!result)
                return NotFound("Operations Provider not found.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
