using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class CreateProductBrand
{
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;

    protected ProductBrandDto ProductBrand { get; set; } = new ProductBrandDto();
   
    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected async Task Submit()
    {
        if (!string.IsNullOrWhiteSpace(ProductBrand.BrandName))
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync("api/productbrand", ProductBrand);

                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Product brand added successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(ProductBrand));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to add product brand: {error}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
        }
        else
        {
            Snackbar.Add("Brand Name is required.", Severity.Warning);
        }
    }

}
