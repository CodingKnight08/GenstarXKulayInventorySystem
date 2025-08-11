using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrderItems;

public partial class CreatePurchaseOrderItem
{
    [Parameter] public BranchOption Branch { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected List<ProductBrandDto> ProductBrands { get; set; } = new();
    protected PurchaseOrderItemDto PurchaseOrderItemDto { get; set; } = new();

    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected string SelectedProduct { get; set; } = string.Empty;
    protected bool IsValid => PurchaseOrderItemDto.ProductBrandId.HasValue && PurchaseOrderItemDto.ProductId.HasValue && PurchaseOrderItemDto.PurchaseItemMeasurementOption.HasValue && PurchaseOrderItemDto.ItemAmount != 0 && PurchaseOrderItemDto.ItemQuantity !=0;
    protected bool IsLoading { get; set; } = true;
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;

        await GetAllProductBrands();
        

        IsLoading = false;
    }
    protected async Task GetAllProductBrands()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/productbrand/all/brands/{Branch}");
            response.EnsureSuccessStatusCode();
            var brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>();
            ProductBrands = brands ?? new List<ProductBrandDto>();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading product brands: {ex.Message}", Severity.Error);
        }
    }


    protected void Submit()
    {
        try
        {
            MudDialog.Close(DialogResult.Ok(PurchaseOrderItemDto));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error creating purchase order item: {ex.Message}", Severity.Error);
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected Task<IEnumerable<string>> SearchBrand(string value, CancellationToken cancellationToken)
    {
        if (ProductBrands == null || !ProductBrands.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        var matchBrand = ProductBrands
            .Where(b => !string.IsNullOrWhiteSpace(b.BrandName) &&
                        (string.IsNullOrWhiteSpace(value) || b.BrandName.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .Select(b => b.BrandName);

        return Task.FromResult(matchBrand.AsEnumerable());
    }



    protected void OnBrandSelected(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand) && ProductBrands == null)
            return;

        SelectedBrand = ProductBrands.FirstOrDefault(b =>
            !string.IsNullOrWhiteSpace(b.BrandName) &&
            string.Equals(b.BrandName, brand, StringComparison.OrdinalIgnoreCase)
        );

        if (SelectedBrand != null)
        {
            PurchaseOrderItemDto.ProductBrand = SelectedBrand;
            PurchaseOrderItemDto.ProductBrandId = SelectedBrand.Id;
        }
        else
        {
            PurchaseOrderItemDto.ProductBrand = null;
            PurchaseOrderItemDto.ProductBrandId = null;
        }

        // Reset product when brand changes
        SelectedProduct = string.Empty;
        PurchaseOrderItemDto.Product = null;
        PurchaseOrderItemDto.ProductId = null;
    }


    protected Task<IEnumerable<string>> SearchProduct(string value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value) || SelectedBrand?.Products == null)
            return Task.FromResult(Enumerable.Empty<string>());

        var matchProduct = SelectedBrand.Products
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName) &&
                        p.ProductName.Contains(value, StringComparison.OrdinalIgnoreCase))
            .Select(p => p.ProductName);

        return Task.FromResult(matchProduct);
    }



    protected void OnProductSelected(string product)
    {
        if (string.IsNullOrWhiteSpace(product) && ProductBrands == null)
            return;

        SelectedProduct = product;

        var selectedProduct = SelectedBrand?.Products?
            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ProductName) &&
                                 string.Equals(p.ProductName, product, StringComparison.OrdinalIgnoreCase));

        if (selectedProduct != null)
        {
            PurchaseOrderItemDto.Product = selectedProduct;
            PurchaseOrderItemDto.ProductId = selectedProduct.Id;
            PurchaseOrderItemDto.PurchaseItemMeasurementOption = selectedProduct.ProductMesurementOption;
        }
        else
        {
            PurchaseOrderItemDto.Product = null;
            PurchaseOrderItemDto.ProductId = null;
        }
    }

}
