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
    [Inject] protected UserState UserState { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;


    protected ProductDto NewProduct { get; set; } = new ProductDto();
    
    protected List<ProductCategoryDto> Categories { get; set; } = new List<ProductCategoryDto>();
    protected bool IsLoading = false;
    protected bool IsDisabled => string.IsNullOrWhiteSpace(NewProduct.ProductName)
                                || NewProduct.Size.Equals(0)
                                || NewProduct.CostPrice.Equals(0)
                                || NewProduct.ProductMesurementOption == null
                                || NewProduct.RetailPrice == null
                                || NewProduct.CostPrice > NewProduct.WholesalePrice
                                || NewProduct.RetailPrice < NewProduct.CostPrice
                                || NewProduct.RetailPrice < NewProduct.WholesalePrice
                                || NewProduct.BufferStocks == null
                                || NewProduct.BufferStocks == 0;
                        


    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadCategoriesAsync();
        NewProduct.Branch = UserState.Branch.GetValueOrDefault();
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
            Snackbar.Add("Failed to load categories. Please try again later.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
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

  


}
