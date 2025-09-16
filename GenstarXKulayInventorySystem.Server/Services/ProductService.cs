using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Security.Claims;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
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
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductCategory)
            .Where(p => p.BrandId == brandId && !p.IsDeleted)
            .OrderBy(e => e.ProductName)
            .ToListAsync() ?? new List<Product>(); ;

        
        return products.Select(product => _mapper.Map<ProductDto>(product)).ToList();
    }
    public async Task<List<ProductDto>> GetAllProductByBrandAndBranch(int brandId, BranchOption branch)
    {
        var products = await _context.Products
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.BrandId == brandId
                     && p.Branch == branch
                     && !p.IsDeleted
                     && p.ActualQuantity > p.BufferStocks)
            .ToListAsync();

        if (products.Count == 0)
            return new List<ProductDto>();

        return products.Select(product => _mapper.Map<ProductDto>(product)).ToList();
    }

    public async Task<List<ProductDto>> GetAllProductsAsyncByBranch(int brandId, BranchOption branch, int skip, int take)
    {
        var products = await _context.Products
           .AsNoTracking()
           .AsSplitQuery()
           .Where(p => p.BrandId == brandId
                    && p.Branch == branch
                    && !p.IsDeleted
                    )
           .ToListAsync();

        if (products.Count == 0)
            return new List<ProductDto>();

        return _mapper.Map<List<ProductDto>>(products).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductCategory)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<bool> AddAsync(ProductDto productDto)
    {
        try
        {
            var existingProduct = await _context.Products.AsNoTracking().AsSplitQuery()
                .FirstOrDefaultAsync(x => x.ProductName == productDto.ProductName &&
                                          x.BrandId == productDto.BrandId && x.Size == productDto.Size && x.ProductMesurementOption == productDto.ProductMesurementOption && x.Branch == productDto.Branch);
            if (existingProduct != null)
                return false;

            productDto.CreatedBy = GetCurrentUsername();
            productDto.CreatedAt = DateTime.UtcNow;
            productDto.ActualQuantity = productDto.Quantity;
            var product = _mapper.Map<Product>(productDto);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            // Use logging here if available
            throw new Exception($"Error adding product: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public async Task<bool> UpdateAsync(ProductDto productDto)
    {
        try
        {
            var existingProduct = await _context.Products
                .Include(p => p.ProductBrand)
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.Id == productDto.Id);

            if (existingProduct == null)
                return false;

            existingProduct.UpdatedAt = DateTime.UtcNow;
            existingProduct.Quantity = (int)Math.Floor(productDto.ActualQuantity);

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


    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        _context.Products.Update(product);
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
        var brands = await _context.ProductBrands.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted ).Skip(skip).Take(take).OrderBy(p => p.BrandName).ToListAsync() ?? new List<ProductBrand>();
        return brands.Select(brand => _mapper.Map<ProductBrandDto>(brand)).ToList();
    }

    public async Task<List<ProductBrandDto>> GetAllBrands()
    {
        var brands = await _context.ProductBrands.AsNoTracking().AsSplitQuery().Where(e => !e.IsDeleted).ToListAsync() ?? new List<ProductBrand>();
        return _mapper.Map<List<ProductBrandDto>>(brands);
    }
    public async Task<List<ProductBrandDto>> GetAllBrandsWithProductsAsync(BranchOption branch)
    {
        var brands = await _context.ProductBrands
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.Products.Where(p => !p.IsDeleted && p.Branch == branch))
            .Where(b => !b.IsDeleted)
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
        .Where(p => p.BrandId == id && !p.IsDeleted)
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

}
public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(int brandId);
    Task<List<ProductDto>> GetAllProductByBrandAndBranch(int brandId, BranchOption branch);
    Task<List<ProductDto>> GetAllProductsAsyncByBranch(int brandId, BranchOption branch, int skip, int take);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<bool> AddAsync(ProductDto productDto);
    Task<bool> UpdateAsync(ProductDto productDto);
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
}