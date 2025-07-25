using GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.Product;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class ViewBrand
{
    [Parameter] public int BrandId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    protected ProductBrandDto Brand { get; set; } = new ProductBrandDto();
    protected List<ProductDto> Products { get; set; } = new List<ProductDto>();
    protected List<ProductCategoryDto> Categories { get; set; } = new List<ProductCategoryDto>();

    
    protected bool IsLoading = false;
    protected string? ErrorMessage { get; set; }
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
      
        await LoadBrandAsync();
        await LoadProductsAsync();
        await LoadCategoriesAsync();
        await Task.Delay(1000); 

        IsLoading = false;
    }
    protected List<BreadcrumbItem> items =
   [
       new("Products", href: "/products"),
        new("Brand Detail", href: "#", disabled:true),
   ];


    protected async Task LoadBrandAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/productbrand/{BrandId}");
            response.EnsureSuccessStatusCode();
            Brand = await response.Content.ReadFromJsonAsync<ProductBrandDto>() ?? new ProductBrandDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching brand: {ex.Message}");
            ErrorMessage = "Failed to load brand details. Please try again later.";
        }
    }

    protected async Task LoadProductsAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/{BrandId}");
            response.EnsureSuccessStatusCode();
            Products = await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new List<ProductDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching products: {ex.Message}");
            ErrorMessage = "Failed to load products. Please try again later.";
        }
    }
    protected async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/productcategory/all");
            response.EnsureSuccessStatusCode();
            Categories = await response.Content.ReadFromJsonAsync<List<ProductCategoryDto>>() ?? new List<ProductCategoryDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching categories: {ex.Message}");
            ErrorMessage = "Failed to load categories. Please try again later.";
        }
    }
   
    
    protected async Task AddProduct()
    {
       var dialogRef = await DialogService.ShowAsync<CreateProduct>("Add Product", new DialogParameters { ["BrandId"] = BrandId });
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled && result.Data is ProductDto)
            {
                await LoadProductsAsync();
                StateHasChanged();
            }
        }
    }

}
