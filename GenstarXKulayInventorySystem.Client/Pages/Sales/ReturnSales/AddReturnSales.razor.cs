using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static MudBlazor.Icons.Custom;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.ReturnSales;

public partial class AddReturnSales
{
    [Parameter] public int SalesId { get; set; }
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;

    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected ILogger<AddReturnSales> Logger { get; set; } = default!;
    protected ReturnItemDto ReturnItem { get; set; } = new ReturnItemDto();
    protected List<ProductBrandDto> ProductBrands { get; set; } = new List<ProductBrandDto>();
    protected List<BranchProductDto> BranchProducts { get; set; } = new List<BranchProductDto>();
    protected BranchProductDto? SelectedProduct { get; set; } = new BranchProductDto();
    protected string BrandName { get; set; } = string.Empty;
    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected bool IsProcessing { get; set; } = false;
    protected bool IsLoading { get; set; } = false;
    protected bool IsProductLoading { get; set; } = false;
    protected decimal PriceItem { get; set; } = 0;
    protected bool IsValid => ReturnItem.BranchProductId != null && ReturnItem.Quantity > 0 && ReturnItem.TotalPrice > 0;
    protected override async Task OnParametersSetAsync()
    {
        await LoadProductBrands();
    }
    protected async Task LoadProductBrands()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/productbrand/all/brandnames");
            response.EnsureSuccessStatusCode();
            var productBrands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>();
            if (productBrands != null)
            {
                ProductBrands = productBrands;
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading product brands");
        }
        finally
        {
            IsLoading = false;
        }

    }
    private async Task LoadBrandProducts()
    {
        IsProductLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/products/by/{SelectedBrand?.Id}/{UserState.Branch}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                BranchProducts = new List<BranchProductDto>();
                Snackbar.Add("Products for the selected brand is low.", Severity.Warning);
                return;
            }
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>();
            BranchProducts = products ?? new List<BranchProductDto>();

        }
        catch (Exception ex)
        {
            Logger.LogError($"{ex.Message}");
            Snackbar.Add("Error occured", Severity.Warning);

        }
        finally
        {
            IsProductLoading = false;
        }
    }
    protected Task<IEnumerable<ProductBrandDto>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        if (ProductBrands == null || ProductBrands.Count == 0)
            return Task.FromResult(Enumerable.Empty<ProductBrandDto>());

        var query = value?.Trim() ?? string.Empty;

        var result = ProductBrands
            .Where(b => string.IsNullOrWhiteSpace(value) ||
            b.BrandName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .GroupBy(b => b.Id)
            .Select(g => g.First());


        return Task.FromResult(result);
    }

    protected Task<IEnumerable<BranchProductDto>> SearchProductsDto(string value, CancellationToken cancellationToken)
    {
        if (BranchProducts is null || !BranchProducts.Any())
            return Task.FromResult(Enumerable.Empty<BranchProductDto>());

        var result = BranchProducts
                  .Where(p => string.IsNullOrWhiteSpace(value) ||
                              ((p.MasterProduct?.ProductName ?? string.Empty)
                                  .Contains(value, StringComparison.OrdinalIgnoreCase)))
                  .GroupBy(p => p.Id)
                  .Select(g => g.First());


        return Task.FromResult(result);
    }
    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        if (brand is null)
        {
            SelectedBrand = null;
            BranchProducts.Clear();
            SelectedProduct = null;
            ReturnItem.BranchProductId = null;

            return;
        }

        SelectedBrand = brand;
        BrandName = brand.BrandName ?? string.Empty;

        // Clear old selections
        SelectedProduct = null;
        await LoadBrandProducts();
    }

    protected void OnProductSelectDto(BranchProductDto product)
    {
        if (product is null)
        {
            SelectedProduct = null;
            ReturnItem.BranchProductId = null;
            ReturnItem.ItemName = string.Empty;
            return;
        }

        SelectedProduct = product;
        ReturnItem.ItemName = product.MasterProduct?.ProductName ?? string.Empty;
        ReturnItem.BranchProductId = product.Id;
        PriceItem = product.RetailPrice ?? 0;
        
        RecalculateTotalPrice();

    }
    protected void OnQuantityChanged(decimal newQty)
    {
        ReturnItem.Quantity = newQty;
        RecalculateTotalPrice();
    }
    private void RecalculateTotalPrice()
    {
        var size = ReturnItem.Size;
        var qty = ReturnItem.Quantity > 0 ? ReturnItem.Quantity : 1;

        ReturnItem.TotalPrice = PriceItem * size * qty;
        StateHasChanged();
    }
    protected async Task PriceChange(decimal price)
    {
        PriceItem = price;
        RecalculateTotalPrice();
        await Task.CompletedTask;
    }
    protected void SaveItem()
    {
        if (!IsValid)
            return;
        IsProcessing = true;
        try
        {
            ReturnItem.DailySaleId = SalesId;
            ReturnItem.ItemPrice = PriceItem;
            Snackbar.Add("Return item added successfully.", Severity.Success);
            MudDialog.Close(DialogResult.Ok(ReturnItem));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding return item");
            Snackbar.Add("Error adding return item.", Severity.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }
    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
