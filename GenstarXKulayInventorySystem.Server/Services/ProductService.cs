using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;


namespace GenstarXKulayInventorySystem.Server.Services;

public class ProductService:IProductService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProductService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
    }
    //Product Methods
    public async Task<List<ProductDto>> GetAllAsync(int brandId)
    {
        var products = await _context.Products.AsNoTracking().AsSplitQuery()
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductCategory)
            .Where(p => p.BrandId == brandId)
            .ToListAsync() ?? new List<Product>(); ;

        
        return products.Select(product => _mapper.Map<ProductDto>(product)).ToList();
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
                                          x.BrandId == productDto.BrandId);
            if (existingProduct != null)
                return false;

            productDto.CreatedBy = GetCurrentUsername();
            productDto.CreatedAt = UtilitiesHelper.GetPhilippineTime();

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
        var existingProduct = await _context.Products
            .Include(p => p.ProductBrand)
            .Include(p => p.ProductCategory)
            .FirstOrDefaultAsync(p => p.Id == productDto.Id);

        if (existingProduct != null)
            return false;

        _mapper.Map(productDto, existingProduct);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    //ProductBrand Methods

    public async Task<List<ProductBrandDto>> GetAllBrandsAsync()
    {
        var brands = await _context.ProductBrands.ToListAsync() ?? new List<ProductBrand>();
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
        brandDto.CreatedAt = UtilitiesHelper.GetPhilippineTime();
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
        existingBrand.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
        _mapper.Map(brandDto, existingBrand);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBrandAsync(int id)
    {
        var brand = await _context.ProductBrands.FindAsync(id);
        if (brand == null)
            return false;

        
        brand.DeletedAt = UtilitiesHelper.GetPhilippineTime();
        _context.ProductBrands.Remove(brand);
        await _context.SaveChangesAsync();
        return true;
    }

    //ProductCategory Methods

    public async Task<List<ProductCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.ProductCategories.ToListAsync() ?? new List<ProductCategory>();
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
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(int brandId);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<bool> AddAsync(ProductDto productDto);
    Task<bool> UpdateAsync(ProductDto productDto);
    Task<bool> DeleteAsync(int id);


    Task<List<ProductBrandDto>> GetAllBrandsAsync();
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