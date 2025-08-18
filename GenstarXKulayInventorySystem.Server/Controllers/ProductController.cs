using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/products
    [HttpGet("all/{brandId}")]
    public async Task<ActionResult<List<ProductDto>>> GetAll(int brandId)
    {
        var products = await _productService.GetAllAsync(brandId);
        return Ok(products);
    }

    [HttpGet("all/by/{brandId:int}/{branch}")]
    public async Task<ActionResult<List<ProductDto>>> GetProductsByBrandAndBranch(int brandId, BranchOption branch)
    {
        try
        {
            var products = await _productService.GetAllProductByBrandAndBrand(brandId, branch);

            if (products == null || !products.Any())
                return NotFound("No products found for the selected brand and branch.");

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto?>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        try
        {
            var result = await _productService.AddAsync(dto);
            if (!result)
                return BadRequest("Product already exists.");

            return Ok(result);
        }
        catch (Exception ex)
        {
           
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var result = await _productService.UpdateAsync(dto);
        if (!result)
            return NotFound();

        return Ok();
    }

    // DELETE: api/products/5
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
