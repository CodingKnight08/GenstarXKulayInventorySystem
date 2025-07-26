using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class DeleteProductBrand
{
    [Parameter] public int ProductBrandId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] public IMudDialogInstance DialogService { get; set; } = default!;

    protected ProductBrandDto DeletedProductBrand { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }
    protected override async Task OnInitializedAsync()
    {
        await LoadProductBrandAsync();
    }
    protected async Task LoadProductBrandAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync($"api/productbrand/{ProductBrandId}");
            response.EnsureSuccessStatusCode();
            DeletedProductBrand = await response.Content.ReadFromJsonAsync<ProductBrandDto>() ?? new ProductBrandDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product brand: {ex.Message}");
            ErrorMessage = "Failed to load product brand. Please try again later.";
        }
        IsLoading = false;
    }

    protected async Task DeleteProductBrandAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/productbrand/{ProductBrandId}");
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product brand deleted successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                ErrorMessage = "Failed to delete product brand. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting product brand: {ex.Message}");
            ErrorMessage = "Failed to delete product brand. Please try again later.";
        }
        await Task.Delay(5000);
        IsLoading = false;
    }

    protected void Cancel()
    {
        DialogService.Cancel();
    }
}
