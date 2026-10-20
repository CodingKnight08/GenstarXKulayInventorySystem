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


    protected List<ProductBrandDto> Brands { get; set; } = new List<ProductBrandDto>();
    protected List<BranchProductDto> Products { get; set; } = new List<BranchProductDto>();
    protected ProductBrandDto SelectedBrand { get; set; } = new ProductBrandDto();
    protected BranchProductDto SelectedProduct { get; set; } = new BranchProductDto();
    protected string BrandName { get; set; } = string.Empty;
    protected InvolvePaintsDto AddedPaint { get; set; } = new InvolvePaintsDto();

    protected bool IsLoading { get; set; } = false;
    protected bool IsProductLoading { get; set; } = false;
    protected bool IsValid =>
    AddedPaint.BrandId != 0 &&
    AddedPaint.ProductId != 0 &&
    (AddedPaint.Size ?? 0) > 0;


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
            var brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>();
            Brands = brands ?? new List<ProductBrandDto>();
        }
        catch (Exception ex) {
            Logger.LogError($"Error in loading brands: {ex.Message}");

        }
        IsLoading = false;
    }

    private async Task LoadProductsByBrand()
    {
        IsProductLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/products/by/{SelectedBrand.Id}/{Branch}");
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>();
            Products = products ?? new List<BranchProductDto>();
        }
        catch (Exception ex) { 
            Logger.LogError(ex.Message);
        }
        finally
        {
            IsProductLoading = false;
        }
    }
    protected Task<IEnumerable<ProductBrandDto>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        if (Brands == null || Brands.Count == 0)
            return Task.FromResult(Enumerable.Empty<ProductBrandDto>());

        var query = value?.Trim() ?? string.Empty;

        var result = Brands
            .Where(b => string.IsNullOrWhiteSpace(value) ||
            b.BrandName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .GroupBy(b => b.Id)
            .Select(g => g.First());


        return Task.FromResult(result);
    }

    protected Task<IEnumerable<BranchProductDto>> SearchProducts(string value, CancellationToken cancellationToken)
    {
        if (Products is null || !Products.Any())
            return Task.FromResult(Enumerable.Empty<BranchProductDto>());

        var result = Products
            .Where(p => string.IsNullOrWhiteSpace(value) ||
                        p.MasterProduct.ProductName.Contains(value, StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.Id)
            .Select(g => g.First());

        return Task.FromResult(result);
    }

    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        if (brand is null)
        {
            SelectedBrand = new ProductBrandDto();
            Products.Clear();
            SelectedProduct = new BranchProductDto();
            AddedPaint.ProductId = 0;
            return;
        }

        SelectedBrand = brand;
        BrandName = brand.BrandName;

        // Clear old selections
        SelectedProduct = new BranchProductDto();
        AddedPaint.BrandId = brand.Id;
        AddedPaint.ProductId = 0;
        AddedPaint.BrandName = brand.BrandName;


        await LoadProductsByBrand();
    }


    protected void OnProductSelect(BranchProductDto product)
    {

        if (product is null || Products is null)
            return;

       SelectedProduct = product;

        if (SelectedProduct.Id != 0)
        {
            AddedPaint.ProductId = SelectedProduct.Id;
            AddedPaint.ProductName = SelectedProduct.MasterProduct?.ProductName ?? string.Empty;
            AddedPaint.ProductCost = SelectedProduct.CostPrice ?? 0;
            AddedPaint.ProductUnit = SelectedProduct.ProductMesurementOption ?? ProductMesurementOption.Gallon;
            AddedPaint.UnitMeasurement = SelectedProduct.ProductMesurementOption ?? ProductMesurementOption.Gallon;
            AddedPaint.CostPrice = SelectedProduct.CostPrice ?? 0;
            StateHasChanged();
        }
    }
    protected void Add()
    {
        if (AddedPaint.BrandId == 0 || AddedPaint.ProductId == 0)
        {
            Snackbar.Add("Please select both a brand and a product before adding.", Severity.Warning);
            return;
        }

        Dialog.Close(DialogResult.Ok(AddedPaint));
    }
    protected void Cancel()
    {
        Dialog.Cancel();
    }
    protected void OnSizeChanged(decimal? size)
    {
        AddedPaint.Size = size;
        ComputeTotalPrice();
    }
    protected void OnQuantityChanged(decimal quantity)
    {
        AddedPaint.Quantity = quantity;
        ComputeTotalPrice();
    }
    protected void ComputeTotalPrice()
    {
        AddedPaint.ProductCost = (AddedPaint.Size ?? 0) * (SelectedProduct.CostPrice ?? 0) * (AddedPaint.Quantity);
        StateHasChanged();
    }
    private decimal GetMaxQuantity()
    {
        return SelectedProduct.ActualQuantity;
    }
}
