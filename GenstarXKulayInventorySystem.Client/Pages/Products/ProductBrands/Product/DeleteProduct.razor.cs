using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.Product;

public partial class DeleteProduct
{
    [Parameter] public int ProductId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] public IMudDialogInstance DialogService { get; set; } = default!;
    protected ProductDto DeletedProduct { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }
    protected override async Task OnInitializedAsync()
    {
        await LoadProductAsync();
    }
    protected async Task LoadProductAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync($"api/product/{ProductId}");
            response.EnsureSuccessStatusCode();
            DeletedProduct = await response.Content.ReadFromJsonAsync<ProductDto>() ?? new ProductDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product: {ex.Message}");
            ErrorMessage = "Failed to load product. Please try again later.";
        }
        IsLoading = false;
    }
    protected async Task DeleteProductAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/product/{ProductId}");
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product deleted successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                ErrorMessage = "Failed to delete product. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting product: {ex.Message}");
            ErrorMessage = "Failed to delete product. Please try again later.";
        }
        await Task.Delay(1000);
        IsLoading = false;
    }
    protected void Cancel()
    {
        DialogService.Cancel();
    }
}
