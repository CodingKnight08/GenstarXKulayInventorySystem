using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.Product;

public partial class CreateProduct
{
    [Parameter] public int BrandId { get; set; }
    [Inject] private UserState UserState { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;


    protected GlobalProductDto NewProduct { get; set; } = new GlobalProductDto();
    protected ProductMesurementOption Unit { get; set; }
   
    protected bool IsLoading = false;
    protected bool IsDisabled => string.IsNullOrWhiteSpace(NewProduct.ProductName);
                           


    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        
    }

   
    protected void Cancel()
    {
       MudDialog.Cancel();
    }

    protected async Task Submit()
    {
        if (!string.IsNullOrWhiteSpace(NewProduct.ProductName) && BrandId > 0)
        {
            try
            {
                NewProduct.BrandId = BrandId;
               
                var response = await HttpClient.PostAsJsonAsync("api/product", NewProduct);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Product added successfully!", Severity.Success);
                    MudDialog.Close(DialogResult.Ok(NewProduct));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Snackbar.Add($"Failed to add product: {error}", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
        }
        else
        {
            Snackbar.Add("Product Name, Brand, and Category are required.", Severity.Warning);
        }
    }

    private void OnUnitChanged(ProductMesurementOption value)
    {
        Unit = value;

        var unitLabel = ProductUnit(value);

        if (string.IsNullOrWhiteSpace(NewProduct.ProductName))
            return;

        // Remove existing (...) if user changes unit again (important)
        var baseName = RemoveUnitSuffix(NewProduct.ProductName);

        NewProduct.ProductName = $"{baseName} {unitLabel}";
    }
    private string RemoveUnitSuffix(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        var index = name.LastIndexOf('(');

        if (index > 0 && name.EndsWith(")"))
            return name[..index].Trim();

        return name;
    }

}
