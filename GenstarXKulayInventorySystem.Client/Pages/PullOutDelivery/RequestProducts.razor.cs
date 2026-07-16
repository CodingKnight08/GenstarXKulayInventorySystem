using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static MudBlazor.Icons.Custom;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class RequestProducts
{
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public BranchOption BranchSource { get; set; }
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected ILogger<RequestProducts> Logger { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    // Local fields
    protected int SelectedProductId { get; set; }
    protected int Quantity { get; set; } = 1;
    protected RequestProductItemDto RequestedProduct { get; set; } = new RequestProductItemDto();
    protected List<SourceAndRequesteeProductDto> Products { get; set; } = new List<SourceAndRequesteeProductDto>();
    protected List<ProductBrandDto> Brands { get; set; } = new List<ProductBrandDto>();
    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected SourceAndRequesteeProductDto? SelectedBranchProduct { get; set; } = new SourceAndRequesteeProductDto();
    protected BranchOption RequesteeBranch { get; set; }
    protected bool IsLoading { get; set; } = false;
    protected bool IsProductsLoading { get; set; } = false;
    protected bool CanSave =>
            RequestedProduct.MasterProductId != null &&
            RequestedProduct.RequestedQuantity > 0;
    protected override async Task OnInitializedAsync()
    {
        RequesteeBranch = UserState.Branch.GetValueOrDefault();
        await LoadBrands();
    }
    protected override void OnParametersSet()
    {
        RequestedProduct.SourceProduct = BranchSource;
    }
    
    protected async Task LoadBrands()
    {
        IsLoading = true;
        try
        {

            var response = await Http.GetAsync("api/productbrand/all/brandnames");
            response.EnsureSuccessStatusCode();
            var brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>();
            Brands = brands ?? new List<ProductBrandDto>();
        }
        catch(Exception ex) 
        {
            Logger.LogError($"Error in loading brands: {ex.Message}");
            Snackbar.Add("Error occured", Severity.Warning);
        }
        finally
        {
            IsLoading = false;
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

    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        if (brand is null)
        {
            SelectedBrand = null;
            Products.Clear();
            return;
        }

        SelectedBrand = brand;
        SelectedBranchProduct = null;
        await LoadBranchProduct();

    }

    protected async Task LoadBranchProduct()
    {
        IsProductsLoading = true;
        try
        {
            var response = await Http.GetAsync($"api/product/all/pulloutitems/products/{SelectedBrand?.Id}/{RequesteeBranch}/{BranchSource}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Products = new List<SourceAndRequesteeProductDto>();
                Snackbar.Add("Products for the selected brand is low.", Severity.Warning);
                return;
            }
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<SourceAndRequesteeProductDto>>();
            Products = products ?? new List<SourceAndRequesteeProductDto>();
        }
        catch(Exception ex)
        {
            Logger.LogError($"{ex.Message}");
            Snackbar.Add("Error occured", Severity.Warning);
        }
        finally
        {
            IsProductsLoading = false;
        }
    }

    protected Task<IEnumerable<SourceAndRequesteeProductDto>> SearchProductsDto(
      string value,
      CancellationToken cancellationToken)
    {
        if (Products == null || Products.Count == 0)
            return Task.FromResult(Enumerable.Empty<SourceAndRequesteeProductDto>());

        var result = Products
            .Where(p =>
                string.IsNullOrWhiteSpace(value) ||
                (p.MasterProduct?.ProductName?.Contains(
                    value,
                    StringComparison.OrdinalIgnoreCase) ?? false)
            );

        return Task.FromResult(result);
    }

    protected void OnProductSelectDto(SourceAndRequesteeProductDto product)
    {
        if (product is null)
        {
            SelectedBranchProduct = null;
            RequestedProduct.MasterProductId = null;
            RequestedProduct.ProductName = string.Empty;
            RequestedProduct.ProductSourceBranchId = null;
            RequestedProduct.ProductRequesterBranchId = null;
            RequestedProduct.ItemCost = 0;
            return;
        }

        SelectedBranchProduct = product;
        RequestedProduct.MasterProductId = product.MasterProductId;
        RequestedProduct.ProductName = product.MasterProduct?.ProductName ?? string.Empty;
        RequestedProduct.ProductRequesterBranchId = product.RequesterProduct?.Id;
        RequestedProduct.ProductSourceBranchId = product.SourceProduct?.Id;
        RequestedProduct.ItemCost = product.RequesterProduct?.CostPrice ?? 0;
    }

    protected void Save()
    {
        RequestedProduct.Branch = RequesteeBranch;
        MudDialog.Close(DialogResult.Ok(RequestedProduct));
    }
    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
