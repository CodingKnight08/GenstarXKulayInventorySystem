using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.Product;

public partial class EditProduct
{
    [Parameter] public int ProductId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected ProductDto UpdatedProduct { get; set; } = new();
    protected List<ProductCategoryDto> Categories { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }
    protected CultureInfo _en = new("en-US");
    protected bool IsUpdating { get; set; } = false;
    protected bool IsDisabled => string.IsNullOrWhiteSpace(UpdatedProduct.ProductName)
                               || UpdatedProduct.Size.Equals(0)
                                || UpdatedProduct.CostPrice.Equals(0)
                                || UpdatedProduct.ProductMesurementOption == null;
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadProductAsync();
        await LoadCategories();
        await Task.Delay(1000); 
        IsLoading = false;
    }

    protected async Task LoadProductAsync()
    {
        
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync($"api/product/{ProductId}");
            response.EnsureSuccessStatusCode();
            UpdatedProduct = await response.Content.ReadFromJsonAsync<ProductDto>() ?? new ProductDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product: {ex.Message}");
            ErrorMessage = "Failed to load product. Please try again later.";
        }
        
    }

    protected async Task LoadCategories()
    {
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync("api/productcategory/all");
            response.EnsureSuccessStatusCode();
            Categories = await response.Content.ReadFromJsonAsync<List<ProductCategoryDto>>() ?? new List<ProductCategoryDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product categories: {ex.Message}");
            ErrorMessage = "Failed to load product categories. Please try again later.";
        }
    }

    protected async Task UpdateProductAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        IsUpdating = true;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/product/{ProductId}", UpdatedProduct);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product updated successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true)); 
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error updating product: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating product: {ex.Message}");
            Snackbar.Add("Failed to update product. Please try again later.", Severity.Error);
        }
        await Task.Delay(1000); 
        IsUpdating = false;
        IsLoading = false;
    }

    protected void Cancel()
    {
        DialogService.Cancel();
    }
}
