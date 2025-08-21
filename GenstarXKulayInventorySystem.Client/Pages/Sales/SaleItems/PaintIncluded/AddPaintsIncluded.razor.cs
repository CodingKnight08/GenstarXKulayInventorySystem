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
    protected List<ProductDto> Products { get; set; } = new List<ProductDto>();
    protected ProductBrandDto SelectedBrand { get; set; } = new ProductBrandDto();
    protected ProductDto SelectedProduct { get; set; } = new ProductDto();
    protected InvolvePaintsDto AddedPaint { get; set; } = new InvolvePaintsDto();

    protected bool IsLoading { get; set; } = false;
    protected bool IsProductLoading { get; set; } = false;
    protected bool IsValid => AddedPaint.BrandId != 0 && AddedPaint.ProductId != 0 && AddedPaint.Size != null;

    protected override async Task OnInitializedAsync()
    {
        await LoadBrands();
    }

    private async Task LoadBrands()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/productbrand/all");
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
            var response = await HttpClient.GetAsync($"api/product/all/by/{SelectedBrand.Id}/{Branch}");
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            Products = products ?? new List<ProductDto>();
        }
        catch (Exception ex) { 
            Logger.LogError(ex.Message);
        }
        finally
        {
            IsProductLoading = false;
        }
    }
    protected Task<IEnumerable<string>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        if (Brands is null || !Brands.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        var result = Brands
            .Where(b => !string.IsNullOrWhiteSpace(b.BrandName) &&
                       (string.IsNullOrWhiteSpace(value) ||
                        b.BrandName.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .Select(b => (string?)b.BrandName);

        return Task.FromResult(result!);
    }

    protected Task<IEnumerable<string>> SearchProducts(string value, CancellationToken cancellationToken)
    {
        if(Products is null|| !Products.Any())
            return Task.FromResult(Enumerable.Empty<string>());
        
        var result = Products.Where(p => !string.IsNullOrWhiteSpace(p.ProductName) && (string.IsNullOrWhiteSpace(value) || p.ProductName.Contains(value, StringComparison.OrdinalIgnoreCase))).Select(p => p.ProductName);
        return Task.FromResult(result);
    }

    protected async Task OnBrandSelect(string brand)
    {
        if (string.IsNullOrWhiteSpace(brand) || Brands is null || Brands.Count == 0)
            return;

        SelectedBrand = Brands.FirstOrDefault(b =>
             !string.IsNullOrWhiteSpace(b.BrandName) &&
             string.Equals(b.BrandName, brand, StringComparison.OrdinalIgnoreCase))
             ?? new ProductBrandDto();

        if (SelectedBrand is not null && SelectedBrand.Id != 0)
        {
            await LoadProductsByBrand();
            // reset product when brand changes
            SelectedProduct = new ProductDto();
            AddedPaint.ProductId = 0;
            AddedPaint.ProductName = string.Empty;

           

            AddedPaint.BrandId = SelectedBrand.Id;
            AddedPaint.BrandName = SelectedBrand.BrandName;

            StateHasChanged();
        }
    }


    protected void OnProductSelect(string product)
    {
        if (string.IsNullOrWhiteSpace(product) || Products is null)
            return;

        SelectedProduct = Products.FirstOrDefault(p =>
            !string.IsNullOrWhiteSpace(p.ProductName) &&
            string.Equals(p.ProductName, product, StringComparison.OrdinalIgnoreCase)
        ) ?? new ProductDto();

        if (SelectedProduct.Id != 0)
        {
            AddedPaint.ProductId = SelectedProduct.Id;
            AddedPaint.ProductName = SelectedProduct.ProductName;
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
}
