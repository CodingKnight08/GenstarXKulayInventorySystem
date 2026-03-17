using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Security.Claims;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;
using static MudBlazor.Icons.Custom;


namespace GenstarXKulayInventorySystem.Server.Services;

public class ProductService:IProductService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ProductService> _logger;

    public ProductService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<ProductService> logger)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }
    //Product Methods
    public async Task<List<ProductDto>> GetAllAsync(int brandId)
    {
        var products = await _context.Products.AsNoTracking().AsSplitQuery()
            .Include(p => p.ProductCategory)
            .Where(p => !p.IsDeleted)
            .OrderBy(e => e.ProductName)
            .ToListAsync() ?? new List<Product>(); ;

        
        return products.Select(product => _mapper.Map<ProductDto>(product)).ToList();
    }
    public async Task<List<BranchProductDto>> GetAllProductByBrandAndBranch(int brandId, BranchOption branch)
    {
        var products = await _context.BranchProducts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(bp => bp.MasterProduct)
            .Where(p =>
                p.Branch == branch &&
                !p.IsDeleted &&
                p.MasterProduct != null &&
                p.MasterProduct.BrandId == brandId
            )
            .OrderBy(p => p.MasterProduct!.ProductName)  
            .ToListAsync();

        return _mapper.Map<List<BranchProductDto>>(products);
    }
    public async Task<BranchProductPageResultDto<BranchProductDto>> GetPagedProductsByBrandAndBranchAsync(
        int brandId,
        BranchOption branch,
        int skip,
        int take,
        string? search = null)
    {
        var query = _context.BranchProducts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(bp => bp.MasterProduct)
            .Where(p => !p.IsDeleted &&
                        p.Branch == branch &&
                        p.MasterProduct != null &&
                        p.MasterProduct.BrandId == brandId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.MasterProduct!.ProductName.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderBy(p => p.MasterProduct!.ProductName)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var result = _mapper.Map<List<BranchProductDto>>(products);

        return new BranchProductPageResultDto<BranchProductDto>
        {
            Products = result,
            TotalCount = totalCount
        };
    }
    public async Task<List<BranchProductDto>> GetAllProductsForWayBill(int brandId, BranchOption branch)
    {
        var products = await _context.BranchProducts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(bp => bp.MasterProduct)
            .Where(p =>
                p.Branch == branch &&
                !p.IsDeleted &&
                p.MasterProduct != null &&
                p.MasterProduct.BrandId == brandId
            )
            .OrderBy(p => p.MasterProduct!.ProductName)  
            .ToListAsync();

        return _mapper.Map<List<BranchProductDto>>(products);
    }

    public async Task<List<GlobalProductDto>> GetAllGlobalProductsByBrand(int brandId)
    {
        var globalProducts = await _context.GlobalProducts.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted && e.BrandId == brandId).ToListAsync() ?? new List<GlobalProduct>();
        return globalProducts.Select(product => _mapper.Map<GlobalProductDto>(product)).ToList();
    }


    public async Task<List<ProductDto>> GetAllProductsAsyncByBranch(int brandId, BranchOption branch, int skip, int take)
    {
        var products = await _context.Products
           .AsNoTracking()
           .AsSplitQuery()
           .Where(p => p.Branch == branch
                    && !p.IsDeleted
                    )
           .ToListAsync();

        if (products.Count == 0)
            return new List<ProductDto>();

        return _mapper.Map<List<ProductDto>>(products).ToList();
    }

    public async Task<List<GlobalProductDto>> GetAllProductNotInTheBranch(int brandId, BranchOption branch)
    {
        var products = await _context.GlobalProducts
            .AsNoTracking()
            .AsSplitQuery()
            .Where(g =>
                (brandId == 0 || g.BrandId == brandId) &&   // optional brand filter
                !g.BranchProducts.Any(bp => bp.Branch == branch && bp.MasterProductId == g.Id)
            )
            .ToListAsync();
        if(products.Count == 0)
        {
            return new List<GlobalProductDto>();
        }

        return _mapper.Map<List<GlobalProductDto>>(products);
    }
    public async Task<int> GetProductCountAsync(int brandId, BranchOption branch)
    {
        var products = await _context.BranchProducts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.MasterProduct)
            .Where(p =>  p.Branch == branch && !p.IsDeleted).ToListAsync();
        return products.Count();
    }
    public async Task<BranchProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.BranchProducts
            .Include(p => p.MasterProduct)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : _mapper.Map<BranchProductDto>(product);
    }

    public async Task<bool> AddAsync(GlobalProductDto productDto)
    {
        if (string.IsNullOrWhiteSpace(productDto.ProductName))
            return false;

        try
        {
            var normalizedName = productDto.ProductName.Trim();

            var exists = await _context.Products
                .AnyAsync(x => x.ProductName.ToLower() == normalizedName.ToLower());

            if (exists)
                return false;

            productDto.ProductName = normalizedName;
            productDto.CreatedBy = GetCurrentUsername();
            productDto.CreatedAt = DateTime.UtcNow;

            var product = _mapper.Map<GlobalProduct>(productDto);

            _context.GlobalProducts.Add(product);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            // preserve stack trace
            throw new InvalidOperationException("Error adding product", ex);
        }
    }
    public async Task<bool> AddBranchProduct(BranchProductDto branchProduct)
    {
        if(branchProduct.MasterProductId == null)
            return false;
        try
        {
            var exist = await _context.BranchProducts
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync(e => !e.IsDeleted && e.Branch == branchProduct.Branch && e.MasterProductId == branchProduct.MasterProductId);
            if (exist is not null)
                return false;
            branchProduct.CreatedAt = PhilippineTime.Now;
            branchProduct.CreatedBy = GetCurrentUsername();
            var product = _mapper.Map<BranchProduct>(branchProduct);
            _context.BranchProducts.Add(product);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch(Exception ex)
        {
            throw new InvalidOperationException("Error adding branch product", ex);
        }
    }

    public async Task<bool> UpdateAsync(BranchProductDto productDto)
    {
        try
        {
            var existingProduct = await _context.BranchProducts
                .FirstOrDefaultAsync(p => p.Id == productDto.Id);

            if (existingProduct == null)
                return false;

            existingProduct.UpdatedAt = PhilippineTime.Now;
            existingProduct.ActualQuantity = productDto.ActualQuantity;

            _mapper.Map(productDto, existingProduct);

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // log the error properly (ILogger or your logging service)
            _logger.LogError(ex, "Error updating product with Id {ProductId}", productDto.Id);

            return false;
        }
    }
    public async Task<bool> UpdateStocksAsync(
    List<UpdateBranchProductDto> stocks)
    {
        try
        {
            if (stocks == null || !stocks.Any())
                return false;

            var ids = stocks.Select(x => x.BranchProductId).ToList();

            var products = await _context.BranchProducts
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            foreach (var product in products)
            {
                var update = stocks.First(x => x.BranchProductId == product.Id);

                product.BufferStocks = update.BufferStock;
                product.ActualQuantity = update.ActualQuantity;
                product.UpdatedAt = PhilippineTime.Now;
                product.UpdatedBy = GetCurrentUsername();
            }

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating stocks. UpdatedBy: {UpdatedBy}, ProductCount: {Count}",
                GetCurrentUsername(),
                stocks?.Count ?? 0);

            return false;
        }
    }



    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.BranchProducts.FindAsync(id);
        if (product == null)
            return false;
        product.IsDeleted = true;
        product.DeletedAt = PhilippineTime.Now;
        _context.BranchProducts.Update(product);
        await _context.SaveChangesAsync();
        return true;
    }

    //ProductBrand Methods
    public async Task<int> GetAllBrandCount()
    {
        var brands = await _context.ProductBrands.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted ).ToListAsync() ?? new List<ProductBrand>();
        return brands.Count();
    }
    public async Task<List<ProductBrandDto>> GetAllBrandsAsync(int take, int skip)
    {
        var brands = await _context.ProductBrands.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted ).OrderBy(p => p.BrandName).Skip(skip).Take(take).ToListAsync() ?? new List<ProductBrand>();
        return brands.Select(brand => _mapper.Map<ProductBrandDto>(brand)).ToList();
    }

    public async Task<List<ProductBrandDto>> GetAllBrands()
    {
        var brands = await _context.ProductBrands.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted).OrderBy(e => e.BrandName).ToListAsync() ?? new List<ProductBrand>();
        return _mapper.Map<List<ProductBrandDto>>(brands);
    }
    public async Task<List<ProductBrandDto>> GetAllBrandsWithProductsAsync(BranchOption branch)
    {
        var brands = await _context.ProductBrands
            .AsNoTracking()
            .AsSplitQuery()
            .Where(b => !b.IsDeleted)
            .OrderBy(b=> b.BrandName)
            .ToListAsync();
        return brands.Select(brand => _mapper.Map<ProductBrandDto>(brand)).ToList();
    }

    public async Task<ProductBrandDto?> GetBrandByIdAsync(int id)
    {
        var brand = await _context.ProductBrands.AsNoTracking().FirstAsync(x => x.Id == id) ;
        return brand == null ? null : _mapper.Map<ProductBrandDto>(brand);
    }

    public async Task<bool> AddBrandAsync(ProductBrandDto brandDto)
    {
        ProductBrand? existingBrand = await _context.ProductBrands
            .FirstOrDefaultAsync(x => x.BrandName == brandDto.BrandName);
        if (existingBrand != null)
        {
            return false;
        }
        brandDto.CreatedBy = GetCurrentUsername();
        brandDto.CreatedAt = DateTime.UtcNow;
        var brand = _mapper.Map<ProductBrand>(brandDto);
        _ = _context.ProductBrands.Add(brand);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBrandAsync(ProductBrandDto brandDto)
    {
        ProductBrand? existingBrand = await _context.ProductBrands.FirstOrDefaultAsync(x => x.Id == brandDto.Id);
        if (existingBrand == null)
            return false;

        existingBrand.UpdatedBy = GetCurrentUsername();
        existingBrand.UpdatedAt = DateTime.UtcNow;
        _mapper.Map(brandDto, existingBrand);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBrandAsync(int id)
    {
        var brand = await _context.ProductBrands.FindAsync(id);
        if (brand == null)
            return false;

        
        brand.DeletedAt = DateTime.UtcNow;
        brand.IsDeleted = true;

        var associatedProducts = await _context.Products
        .Where(p => !p.IsDeleted)
        .ToListAsync();

        foreach (var product in associatedProducts)
        {
            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
        }
        _context.Products.UpdateRange(associatedProducts);
        _context.ProductBrands.Update(brand);
        await _context.SaveChangesAsync();
        return true;
    }

    //ProductCategory Methods

    public async Task<List<ProductCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.ProductCategories.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted).ToListAsync() ?? new List<ProductCategory>();
        return categories.Select(category => _mapper.Map<ProductCategoryDto>(category)).ToList();
    }

    public async Task<ProductCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.ProductCategories
            .AsNoTracking()
            .FirstAsync(x => x.Id == id);

        return category == null ? null : _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task<bool> AddCategoryAsync(ProductCategoryDto categoryDto)
    {
        ProductCategory? existingCategory = await _context.ProductCategories
            .FirstOrDefaultAsync(x => x.Name == categoryDto.Name);
        if (existingCategory != null)
        {
            return false;
        }
        
        var category = _mapper.Map<ProductCategory>(categoryDto);
        _ = _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCategoryAsync(ProductCategoryDto categoryDto)
    {
        ProductCategory? existingCategory = await _context.ProductCategories
            .FirstOrDefaultAsync(x => x.Id == categoryDto.Id);
        if (existingCategory == null)
            return false;
       
        _mapper.Map(categoryDto, existingCategory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
            return false;

       
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        _context.ProductCategories.Update(category);

        // Find products that reference this category
        var affectedProducts = await _context.Products
            .Where(p => p.ProductCategoryId == id)
            .ToListAsync();

        // Update their CategoryId to 0 (unassigned)
        foreach (var product in affectedProducts)
        {
            product.ProductCategoryId = null;
        }

        _context.Products.UpdateRange(affectedProducts);

        // Save changes
        await _context.SaveChangesAsync();

        return true;
    }


    //PullOut request Item 
    public async Task<List<SourceAndRequesteeProductDto>> GetAllRequesteeAndSourceProduct(
    int brandId,
    BranchOption requester,
    BranchOption source)
    {
        var query =
            from gp in _context.GlobalProducts
                .AsNoTracking()

            where gp.BrandId == brandId

            let sourceProduct = gp.BranchProducts
                .FirstOrDefault(bp => bp.Branch == source)

            let requesterProduct = gp.BranchProducts
                .FirstOrDefault(bp => bp.Branch == requester)

            where sourceProduct != null || requesterProduct != null

            select new SourceAndRequesteeProductDto
            {
                MasterProductId = gp.Id,
                MasterProduct = _mapper.Map<GlobalProductDto>(gp),
                SourceProduct = sourceProduct != null
                    ? _mapper.Map<BranchProductDto>(sourceProduct)
                    : null,
                RequesterProduct = requesterProduct != null
                    ? _mapper.Map<BranchProductDto>(requesterProduct)
                    : null
            };

        return await query.ToListAsync();
    }

}
public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(int brandId);
    Task<List<GlobalProductDto>> GetAllGlobalProductsByBrand(int brandId);
    Task<List<BranchProductDto>> GetAllProductByBrandAndBranch(int brandId, BranchOption branch);
    Task<BranchProductPageResultDto<BranchProductDto>> GetPagedProductsByBrandAndBranchAsync(
        int brandId,
        BranchOption branch,
        int skip,
        int take,
        string? search = null);
    Task<List<BranchProductDto>> GetAllProductsForWayBill(int brandId, BranchOption branch);
    Task<List<ProductDto>> GetAllProductsAsyncByBranch(int brandId, BranchOption branch, int skip, int take);
    Task<List<GlobalProductDto>> GetAllProductNotInTheBranch(int brandId, BranchOption branch);
    Task<int> GetProductCountAsync(int brandId, BranchOption branch);
    Task<BranchProductDto?> GetByIdAsync(int id);
    Task<bool> AddAsync(GlobalProductDto productDto);
    Task<bool> AddBranchProduct(BranchProductDto branchProduct);
    Task<bool> UpdateAsync(BranchProductDto productDto);
    Task<bool> UpdateStocksAsync(
    List<UpdateBranchProductDto> stocks);
    Task<bool> DeleteAsync(int id);

    Task<int> GetAllBrandCount();
    Task<List<ProductBrandDto>> GetAllBrands();
    Task<List<ProductBrandDto>> GetAllBrandsAsync(int take, int skip);
    Task<List<ProductBrandDto>> GetAllBrandsWithProductsAsync(BranchOption branch);
    Task<ProductBrandDto?> GetBrandByIdAsync(int id);
    Task<bool> AddBrandAsync(ProductBrandDto brandDto);
    Task<bool> UpdateBrandAsync(ProductBrandDto brandDto);
    Task<bool> DeleteBrandAsync(int id);


    Task<List<ProductCategoryDto>> GetAllCategoriesAsync();
    Task<ProductCategoryDto?> GetCategoryByIdAsync(int id);
    Task<bool> AddCategoryAsync(ProductCategoryDto categoryDto);
    Task<bool> UpdateCategoryAsync(ProductCategoryDto categoryDto);
    Task<bool> DeleteCategoryAsync(int id);


    Task<List<SourceAndRequesteeProductDto>> GetAllRequesteeAndSourceProduct(int brandId, BranchOption requester, BranchOption source);
}