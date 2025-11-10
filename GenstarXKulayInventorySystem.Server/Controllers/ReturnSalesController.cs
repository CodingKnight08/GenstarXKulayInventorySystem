using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReturnSalesController : ControllerBase
{
    private readonly IReturnSalesService _returnSalesService;
    private readonly ILogger<ReturnSalesController> _logger;

    public ReturnSalesController(IReturnSalesService returnSalesService, ILogger<ReturnSalesController> logger)
    {
        _returnSalesService = returnSalesService;
        _logger = logger;
    }

    [HttpGet("all/{date}/{branch}")]
    public async Task<ActionResult> GetAllReturnSales(DateTime date, BranchOption branch)
    {
        try
        {
            var returnSales = await _returnSalesService.GetAllReturnSalesAsync(branch, date);
            return Ok(returnSales);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting return sales");
            return StatusCode(500, ex.Message);
        }
    }
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetReturnSaleById(int id)
    {
        try
        {
            var returnSale = await _returnSalesService.GetReturnSaleById(id);
            if (returnSale == null)
                return NotFound("Return sale not found.");
            return Ok(returnSale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting return sale by ID");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateReturnSale(ReturnSalesDto returnSalesDto)
    {
        try
        {
            var result = await _returnSalesService.CreateReturnSale(returnSalesDto);
            if (result)
                return Ok("Return sale created successfully.");
            else
                return BadRequest("Failed to create return sale.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating return sale");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteReturnSale(int id)
    {
        try
        {
            var result = await _returnSalesService.DeleteReturnSale(id);
            if (result)
                return Ok("Return sale deleted successfully.");
            else
                return NotFound("Return sale not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting return sale");
            return StatusCode(500, ex.Message);
        }
    }

}
