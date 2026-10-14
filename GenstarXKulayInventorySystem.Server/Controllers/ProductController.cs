using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Authorize]
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
    [HttpGet("all/global/notexisting/{brandId:int}/{branch}")]
    public async Task<ActionResult<List<GlobalProductDto>>> GetNotAddedGlobalProductsInBranch(int brandId, BranchOption branch)
    {
        try
        {

            var products = await _productService.GetAllProductNotInTheBranch(brandId, branch);
            if (products == null || !products.Any())
                return Ok(new List<GlobalProductDto>());
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }

    [HttpGet("all/global/{brandId:int}")]
    public async Task<ActionResult<List<GlobalProductDto>>> GetAllGlobalProducts(int brandId)
    {
        try
        {
            var products = await _productService.GetAllGlobalProductsByBrand(brandId);
            if (products == null || !products.Any())
                return Ok(new List<GlobalProductDto>());
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }

    [HttpGet("paged/by/{brandId:int}/{branch}")]
    public async Task<ActionResult<BranchProductPageResultDto<BranchProductDto>>> GetProductsByBrandAndBranchPaged(
    int brandId,
    BranchOption branch,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 10)
    {
        try
        {
            var products = await _productService.GetAllProductByBrandAndBranch(brandId, branch);

            if (products == null || !products.Any())
                return new BranchProductPageResultDto<BranchProductDto > { Products = new(), TotalCount = 0 };

            var total = products.Count;
            var pagedItems = products.Skip(skip).Take(take).ToList();

            return Ok(new BranchProductPageResultDto<BranchProductDto>
            {
                Products = pagedItems,
                TotalCount = total
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }


    [HttpGet("count/{brandId:int}/{branch}")]
    public async Task<ActionResult<int>> GetProductCount(int brandId, BranchOption branch)
    {
        try
        {
            int count = await _productService.GetProductCountAsync(brandId, branch);
            return Ok(count);
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }

    [HttpGet("all/products/by/{brandId:int}/{branch}")]
    public async Task<ActionResult<List<BranchProductDto>>> GetAllProductByBrandAndBranch(int brandId,BranchOption branch)
    {
        try
        {
            var products = await _productService.GetAllProductByBrandAndBranch(brandId, branch);

            if (products == null || !products.Any())
                return Ok(new List<BranchProductDto>());

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error in retrieving products: {ex.Message}");
        }
    }
    [HttpGet("all/purchase-order/by/{brandId:int}/{branch}")]
    public async Task<ActionResult<List<BranchProductDto>>> GetProductForPO(int brandId, BranchOption branch)
    {
        try
        {
            var products = await _productService.GetProductPurchaseOrderItems(brandId, branch);

            if (products == null || !products.Any())
                return Ok(new List<BranchProductDto>());

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error in retrieving products: {ex.Message}");
        }
    }

    [HttpGet("by/{brandId:int}/{branch}")]
    public async Task<ActionResult<BranchProductPageResultDto<BranchProductDto>>> GetPagedProductsByBrandAndBranch(
    int brandId,
    BranchOption branch,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 10,
    [FromQuery] string? search = null)
    {
        try
        {
            var result = await _productService.GetPagedProductsByBrandAndBranchAsync(
                brandId, branch, skip, take, search);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error in retrieving products: {ex.Message}");
        }
    }
    [HttpGet("all/existing/products/{brandId:int}/{branch}")]
    public async Task<ActionResult<List<BranchProductDto>>> GetProductsForWayBill(int brandId, BranchOption branch)
    {
        try
        {
            var products = await _productService.GetAllProductsForWayBill(brandId, branch);
            if(products == null || !products.Any())
                return Ok(new List<BranchProductDto>());
            return Ok(products);
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }

    [HttpGet("all/stocks/{branch}")]
    public async Task<ActionResult<List<StocksDto>>> GetAllProductsByBranch(
      BranchOption branch,
      [FromQuery] bool isBrand = false,
      [FromQuery] string? searchText = null)
    {
        try
        {
            var products = await _productService.GetAllProductsByStore(branch, isBrand, searchText);

            return Ok(products ?? new List<StocksDto>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving products: {ex.Message}");
        }
    }

    // GET: api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BranchProductDto?>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<IActionResult> Create(GlobalProductDto dto)
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
    [HttpPost("branch")]
    public async Task<IActionResult> CreateBranchProduct(BranchProductDto dto)
    {
        try
        {
            var result = await _productService.AddBranchProduct(dto);
            if (!result)
                return BadRequest("Branch Product already exist");
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    // PUT: api/products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BranchProductDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var result = await _productService.UpdateAsync(dto);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPut("update-stocks")]
    public async Task<IActionResult> UpdateStocks(
    [FromBody] List<UpdateBranchProductDto> stocks)
    {
        if (stocks == null || !stocks.Any())
            return BadRequest("No stocks provided");

        var result = await _productService.UpdateStocksAsync(
            stocks);
            

        if (!result)
            return StatusCode(500, "Failed to update stocks");

        return Ok(true);
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



    //PULL OUT REQUEST PRODUCTS 
    [HttpGet("all/pulloutitems/products/{brandId:int}/{requesteeBranch}/{sourceBranch}")]
    public async Task<ActionResult<List<SourceAndRequesteeProductDto>>> GetAllSourceAndRequesterProduct(int brandId, BranchOption requesteeBranch, BranchOption sourceBranch)
    {
        try
        {
            var results = await _productService.GetAllRequesteeAndSourceProduct(brandId, requesteeBranch, sourceBranch);
            if (results == null || !results.Any())
                return Ok(new List<SourceAndRequesteeProductDto>());
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error in retrieving product: {ex.Message}");
        }
    }
}
