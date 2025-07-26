using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoryController : ControllerBase
{

    private readonly IProductService _productService;

    public ProductCategoryController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<ProductCategoryDto>>> GetAll()
    {
        var categories = await _productService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductCategoryDto>> GetById(int id)
    {
        var category = await _productService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<ProductCategoryDto>> Create(ProductCategoryDto dto)
    {
        var result = await _productService.AddCategoryAsync(dto);
        if (!result)
            return BadRequest("Category already exists.");
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductCategoryDto>> Update(int id, ProductCategoryDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Category ID mismatch.");

        var updatedCategory = await _productService.UpdateCategoryAsync(dto);
        if (!updatedCategory)
            return NotFound("Category not found or already exists.");

        return Ok(updatedCategory);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var success = await _productService.DeleteCategoryAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }
}
