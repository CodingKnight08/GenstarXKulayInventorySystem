using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductCategory;

public partial class DeleteProductCategory
{
    [Parameter] public int CategoryId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] public IMudDialogInstance DialogService { get; set; } = default!;
    protected ProductCategoryDto DeletedCategory { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }
    protected override async Task OnInitializedAsync()
    {
        await LoadCategoryAsync();
    }
    protected async Task LoadCategoryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync($"api/productcategory/{CategoryId}");
            response.EnsureSuccessStatusCode();
            DeletedCategory = await response.Content.ReadFromJsonAsync<ProductCategoryDto>() ?? new ProductCategoryDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product category: {ex.Message}");
            ErrorMessage = "Failed to load product category. Please try again later.";
        }
        IsLoading = false;
    }

    protected async Task DeleteCategoryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/productcategory/{CategoryId}");
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product category deleted successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                ErrorMessage = "Failed to delete product category. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting product category: {ex.Message}");
            ErrorMessage = "Failed to delete product category. Please try again later.";
        }
        await Task.Delay(1000);
        IsLoading = false;
    }

    protected void Cancel()
    {
        DialogService.Cancel();
    }
}
