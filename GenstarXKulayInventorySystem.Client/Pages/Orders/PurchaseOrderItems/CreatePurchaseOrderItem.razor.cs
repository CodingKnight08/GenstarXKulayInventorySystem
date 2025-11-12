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
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;

    protected List<ProductBrandDto> ProductBrands { get; set; } = new();
    protected List<ProductDto> Products { get; set; } = new();
    protected PurchaseOrderItemDto PurchaseOrderItemDto { get; set; } = new();

    protected ProductBrandDto? SelectedBrand { get; set; }
    protected ProductDto? SelectedProduct { get; set; }
    protected bool IsLoading { get; set; } = false;

    protected bool IsValid =>
        PurchaseOrderItemDto.ProductBrandId.HasValue &&
        PurchaseOrderItemDto.ProductId.HasValue &&
        PurchaseOrderItemDto.PurchaseItemMeasurementOption.HasValue &&
        PurchaseOrderItemDto.ItemQuantity > 0 &&
        PurchaseOrderItemDto.ItemAmount > 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadBrands();
    }

    private async Task LoadBrands()
    {
        try
        {
            IsLoading = true;
            var response = await HttpClient.GetAsync($"api/productbrand/all/brands/{Branch}");
            response.EnsureSuccessStatusCode();
            ProductBrands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>() ?? new();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading brands: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProductsByBrand(int brandId)
    {
        if (brandId == 0) return;

        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/by/{brandId}/{Branch}");
            if (!response.IsSuccessStatusCode)
            {
                Products.Clear();
                Snackbar.Add("No products found for this brand.", Severity.Warning);
                return;
            }
            Products = await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading products: {ex.Message}", Severity.Error);
        }
    }

    protected Task<IEnumerable<ProductBrandDto>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        var query = value?.Trim() ?? string.Empty;
        var result = ProductBrands
            .Where(b => string.IsNullOrWhiteSpace(value) ||
                        b.BrandName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .GroupBy(b => b.Id)
            .Select(g => g.First());
        return Task.FromResult(result);
    }

    protected Task<IEnumerable<ProductDto>> SearchProducts(string value, CancellationToken cancellationToken)
    {
        if (Products is null || !Products.Any())
            return Task.FromResult(Enumerable.Empty<ProductDto>());

        var result = Products
            .Where(p => string.IsNullOrWhiteSpace(value) ||
                        p.ProductNameAndUnit.Contains(value, StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.Id)
            .Select(g => g.First());
        return Task.FromResult(result);
    }

    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        SelectedBrand = brand;

        PurchaseOrderItemDto.ProductBrandId = brand?.Id;
        PurchaseOrderItemDto.ProductBrand = brand;
        PurchaseOrderItemDto.ProductId = null;
        PurchaseOrderItemDto.Product = null;
        SelectedProduct = null;

        await LoadProductsByBrand(brand.Id);
    }

    protected void OnProductSelect(ProductDto product)
    {
        SelectedProduct = product;

        if (product is null)
        {
            PurchaseOrderItemDto.ProductId = null;
            PurchaseOrderItemDto.Product = null;
            PurchaseOrderItemDto.PurchaseItemMeasurementOption = null;
            return;
        }

        PurchaseOrderItemDto.ProductId = product.Id;
        PurchaseOrderItemDto.Product = product;
        PurchaseOrderItemDto.PurchaseItemMeasurementOption = product.ProductMesurementOption;
    }

    protected void Submit()
    {
        try
        {
            MudDialog.Close(DialogResult.Ok(PurchaseOrderItemDto));
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error submitting item: {ex.Message}", Severity.Error);
        }
    }

    protected void Cancel() => MudDialog.Cancel();
}
