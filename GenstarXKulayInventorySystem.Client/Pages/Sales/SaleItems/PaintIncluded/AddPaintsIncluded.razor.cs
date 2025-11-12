using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems.PaintIncluded;

public partial class AddPaintsIncluded
{
    [Parameter] public BranchOption Branch { get; set; }
    [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<AddPaintsIncluded> Logger { get; set; } = default!;

    protected List<ProductBrandDto> Brands { get; set; } = new();
    protected List<ProductDto> Products { get; set; } = new();
    protected ProductBrandDto SelectedBrand { get; set; } = new();
    protected ProductDto SelectedProduct { get; set; } = new();
    protected InvolvePaintsDto AddedPaint { get; set; } = new();
    private decimal PriceItem { get; set; } = 0;
    protected bool IsLoading { get; set; }
    protected bool IsProductLoading { get; set; }

    protected bool IsValid =>
        AddedPaint.BrandId != 0 &&
        AddedPaint.ProductId != 0 &&
        (AddedPaint.Size ?? 0) > 0 &&
        AddedPaint.Quantity > 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadBrands();
    }

    private async Task LoadBrands()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/productbrand/all/brandnames");
            response.EnsureSuccessStatusCode();
            Brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>() ?? new();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading brands: {ex.Message}");
        }
        IsLoading = false;
    }

    private async Task LoadProductsByBrand()
    {
        IsProductLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/by/{SelectedBrand.Id}/{Branch}");
            response.EnsureSuccessStatusCode();
            Products = await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? new();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading products: {ex.Message}");
        }
        finally
        {
            IsProductLoading = false;
        }
    }

    protected Task<IEnumerable<ProductBrandDto>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        if (Brands is null || Brands.Count == 0)
            return Task.FromResult(Enumerable.Empty<ProductBrandDto>());

        var query = value?.Trim() ?? string.Empty;
        var result = Brands
            .Where(b => string.IsNullOrWhiteSpace(query) ||
                        b.BrandName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .GroupBy(b => b.Id)
            .Select(g => g.First());

        return Task.FromResult(result);
    }

    protected Task<IEnumerable<ProductDto>> SearchProducts(string value, CancellationToken cancellationToken)
    {
        if (Products is null || !Products.Any())
            return Task.FromResult(Enumerable.Empty<ProductDto>());

        var query = value?.Trim() ?? string.Empty;
        var result = Products
            .Where(p => string.IsNullOrWhiteSpace(query) ||
                        p.ProductNameAndUnit.Contains(query, StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.Id)
            .Select(g => g.First());

        return Task.FromResult(result);
    }

    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        if (brand is null)
        {
            SelectedBrand = new();
            Products.Clear();
            SelectedProduct = new();
            AddedPaint.ProductId = 0;
            return;
        }

        SelectedBrand = brand;
        AddedPaint.BrandId = brand.Id;
        AddedPaint.BrandName = brand.BrandName;
        SelectedProduct = new();
        AddedPaint.ProductId = 0;

        await LoadProductsByBrand();
    }

    protected void OnProductSelect(ProductDto product)
    {
        if (product is null)
            return;

        SelectedProduct = Products.FirstOrDefault(p => p.Id == product.Id) ?? new();

        if (SelectedProduct.Id != 0)
        {
            AddedPaint.ProductId = SelectedProduct.Id;
            AddedPaint.ProductName = SelectedProduct.ProductName;
            AddedPaint.ProductCost = SelectedProduct.CostPrice;
            AddedPaint.ProductUnit = SelectedProduct.ProductMesurementOption ?? ProductMesurementOption.Gallon;
            PriceItem = SelectedProduct.RetailPrice ?? 0;
            AddedPaint.UnitMeasurement = SelectedProduct.ProductMesurementOption ?? ProductMesurementOption.Quart;

            RecalculateTotal();
        }
    }

    private void OnSizeChanged(decimal? size)
    {
        AddedPaint.Size = size;
        RecalculateTotal();
    }
    private void OnValueChanged(decimal value)
    {
        AddedPaint.Quantity = value;
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        var size = AddedPaint.Size ?? 1;
        var quantity = AddedPaint.Quantity == 0 ? 1 : AddedPaint.Quantity;

        AddedPaint.TotalCost = AddedPaint.ProductCost * size * quantity;
        InvokeAsync(StateHasChanged);
    }


    protected void Add()
    {
        if (!IsValid)
        {
            Snackbar.Add("Please fill in all required fields before adding.", Severity.Warning);
            return;
        }

        Dialog.Close(DialogResult.Ok(AddedPaint));
    }

    protected void Cancel() => Dialog.Cancel();
}
