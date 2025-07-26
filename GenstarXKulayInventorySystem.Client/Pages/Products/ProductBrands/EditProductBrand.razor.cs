using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class EditProductBrand
{
    [Parameter] public int ProductBrandId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected ProductBrandDto UpdatedBrand { get; set; } = new();

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
            UpdatedBrand = await response.Content.ReadFromJsonAsync<ProductBrandDto>() ?? new ProductBrandDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product brand: {ex.Message}");
            ErrorMessage = "Failed to load product brand. Please try again later.";
        }
        IsLoading = false;
    }


    protected async Task UpdateProductBrandAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/productbrand/{ProductBrandId}", UpdatedBrand);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product brand updated successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true)); 
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed to update product brand: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating product brand: {ex.Message}");
            ErrorMessage = "Failed to update product brand. Please try again later.";
        }

        IsLoading = false;
    }



    protected void Cancel()
    {
        DialogService.Cancel();
    }
}