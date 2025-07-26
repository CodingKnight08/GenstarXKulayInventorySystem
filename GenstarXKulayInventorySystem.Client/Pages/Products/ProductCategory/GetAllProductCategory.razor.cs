using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductCategory;

public partial class GetAllProductCategory
{

    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllProductCategory> Logger { get; set; } = default!;
    protected List<ProductCategoryDto> Categories { get; set; } = new List<ProductCategoryDto>();
    protected string? ErrorMessage { get; set; }
    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadCategories();
    }

    private async Task LoadCategories()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/productcategory/all");
            response.EnsureSuccessStatusCode();
            var categories = await response.Content.ReadFromJsonAsync<List<ProductCategoryDto>>();
            Categories = categories ?? new List<ProductCategoryDto>();
        }
        catch(Exception ex)
        {  
            Logger.LogError(ex, "Error fetching product categories");
            ErrorMessage = "Failed to load product categories. Please try again later.";
        }
        IsLoading = false;
    }

    protected async Task CreateCategories()
    {
        var dialogRef = await DialogService.ShowAsync<CreateProductCategory>("Add Product Category");
        if (dialogRef != null)
        {
            var result = await dialogRef.Result;
            if(result is not null && !result.Canceled)
            {
                await LoadCategories();
                StateHasChanged();
            }
        }
    }

    protected async Task UpdateCategory(int categoryId)
    {
        var dialogRef = await DialogService.ShowAsync<EditProductCategory>("Update Product Category",
            new DialogParameters { { "ProductCategoryId", categoryId } });
        if (dialogRef != null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                await LoadCategories();
                StateHasChanged();
            }
        }
    }

    protected async Task DeleteCategory(int categoryId)
    {
        var dialogRef = await DialogService.ShowAsync<DeleteProductCategory>("Delete Category",
            new DialogParameters { { "CategoryId", categoryId } });
        if (dialogRef != null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                await LoadCategories();
                StateHasChanged();
            }
        }
    }
}
