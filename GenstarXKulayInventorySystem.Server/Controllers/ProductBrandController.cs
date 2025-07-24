using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class ProductBrandController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductBrandController(IProductService productService)
    {
        _productService = productService;
    }
    [HttpGet("all")]
    public async Task<ActionResult<List<ProductBrandDto>>> GetAll()
    {
        var brands = await _productService.GetAllBrandsAsync();
        return Ok(brands);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductBrandDto>> GetById(int id)
    {
        var brand = await _productService.GetBrandByIdAsync(id);
        if (brand == null)
            return NotFound();
        return Ok(brand);
    }

    [HttpPost]
    public async Task<ActionResult<ProductBrandDto>> Create(ProductBrandDto dto)
    {
        bool result = await _productService.AddBrandAsync(dto);
        if (!result)
            return BadRequest("Brand already exists.");
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductBrandDto>> Update(int id, ProductBrandDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Brand ID mismatch.");

        var updatedBrand = await _productService.UpdateBrandAsync(dto);
        if (!updatedBrand)
            return NotFound("Brand not found or already exists.");

        return Ok(updatedBrand);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _productService.DeleteBrandAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
