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
    protected List<BranchProductDto> Products = new();
    protected int SelectedProductId { get; set; }
    protected int Quantity { get; set; } = 1;
    protected RequestProductItemDto RequestedProduct { get; set; } = new RequestProductItemDto();
    protected List<ProductBrandDto> Brands { get; set; } = new List<ProductBrandDto>();
    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected BranchProductDto? SelectedBranchProduct { get; set; } = new BranchProductDto();
    protected bool IsLoading { get; set; } = false;
    protected bool IsProductsLoading { get; set; } = false;
    protected bool CanSave =>
            RequestedProduct.MasterProductId != null &&
            RequestedProduct.RequestedQuantity > 0;
    protected override async Task OnInitializedAsync()
    {
        await LoadBrands();
        RequestedProduct.Branch = UserState.Branch.GetValueOrDefault();
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
            var response = await Http.GetAsync($"api/product/all/by/{SelectedBrand?.Id}/{BranchSource}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Products = new List<BranchProductDto>();
                Snackbar.Add("Products for the selected brand is low.", Severity.Warning);
                return;
            }
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>();
            Products = products ?? new List<BranchProductDto>();
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

    protected Task<IEnumerable<BranchProductDto>> SearchProductsDto(string value, CancellationToken cancellationToken)
    {
        if (Products is null || !Products.Any())
            return Task.FromResult(Enumerable.Empty<BranchProductDto>());

        var result = Products
                  .Where(p => string.IsNullOrWhiteSpace(value) ||
                              ((p.MasterProduct?.ProductName ?? string.Empty)
                                  .Contains(value, StringComparison.OrdinalIgnoreCase)))
                  .GroupBy(p => p.Id)
                  .Select(g => g.First());


        return Task.FromResult(result);
    }
    protected void OnProductSelectDto(BranchProductDto product)
    {
        if (product is null)
        {
            SelectedBranchProduct = null;
            RequestedProduct.MasterProductId = null;
            RequestedProduct.ProductName = string.Empty;
            return;
        }

        SelectedBranchProduct = product;
        RequestedProduct.MasterProductId = product.MasterProductId;
        RequestedProduct.ProductName = product.MasterProduct?.ProductName ?? string.Empty;
    }

    protected void Save()
    {
        MudDialog.Close(DialogResult.Ok(RequestedProduct));
    }
    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
