using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly ILogger<UserController> _logger;
    public UserController(IUserService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("staffs/{branch}")]
    public async Task<IActionResult> GetStaffs(BranchOption branch)
    {
        var staffs = await _service.GetStaffsAsync(branch);
        return Ok(staffs);
    }

    [HttpPut("assign/staff")]
    public async Task<IActionResult> AssignStaffToBranchProduct([FromBody] BranchProductDto branchProduct)
    {
        var result = await _service.AssignBranchProductStaff(branchProduct);

        if (result)
        {
            return Ok(new { message = "Staff assigned successfully." });
        }

        _logger.LogWarning($"Failed to assign staff to branch product with ID {branchProduct.Id}.");
        return BadRequest(new { message = "Failed to assign staff." });
    }

}
