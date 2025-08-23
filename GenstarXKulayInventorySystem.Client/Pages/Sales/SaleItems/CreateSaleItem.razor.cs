using GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems.PaintIncluded;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems;

public partial class CreateSaleItem
{
    [Parameter] public BranchOption Branch { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<CreateSaleItem> Logger { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    protected SaleItemDto SaleItemDto { get; set; } = new SaleItemDto();
    protected List<ProductBrandDto> ProductBrands { get; set; } = new List<ProductBrandDto>();
    protected List<ProductDto> Products { get; set; } = new List<ProductDto>();
    protected ProductDto? SelectedProductFromList { get; set; } = new ProductDto();
    protected List<InvolvePaintsDto> Paints { get; set; } = new List<InvolvePaintsDto>();

    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected string SelectedProduct { get; set; } = string.Empty;
    protected bool IsLoading { get; set; } = false;
    protected bool IsProductLoading { get; set; } = false;
    protected bool IsWholeSale { get; set; } = false;
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
            ProductBrands = brands ?? new List<ProductBrandDto>();
        }
        catch (Exception ex) {
            Logger.LogError($"Error in loading brands: {ex.Message}");
            Snackbar.Add("Error occured", Severity.Warning);
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
            var response = await HttpClient.GetAsync($"api/product/all/by/{SelectedBrand?.Id}/{Branch}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Products = new List<ProductDto>();
                Snackbar.Add("Products for the selected brand is low.", Severity.Warning);
                return;
            }
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            Products = products ?? new List<ProductDto>();

        }
        catch (Exception ex) { 
            Logger.LogError($"{ex.Message}");
            Snackbar.Add("Error occured", Severity.Warning);

        }
        finally
        {
            IsProductLoading = false;
        }
    }
    protected Task<IEnumerable<string?>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        if (ProductBrands is null || !ProductBrands.Any())
            return Task.FromResult(Enumerable.Empty<string?>());

        var result = ProductBrands
            .Where(b => !string.IsNullOrWhiteSpace(b.BrandName)
                     && (string.IsNullOrWhiteSpace(value)
                     || b.BrandName.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .Select(b => (string?)b.BrandName); // cast to nullable

        return Task.FromResult(result);
    }


    protected Task<IEnumerable<string>> SearchProducts(string value, CancellationToken cancellationToken)
    { if (Products is null || !Products.Any()) 
            return Task.FromResult(Enumerable.Empty<string>()); 
        var result = Products
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName) 
            && (string.IsNullOrWhiteSpace(value) || p.ProductName
            .Contains(value, StringComparison.OrdinalIgnoreCase))).Select(p => p.ProductName);
        return Task.FromResult(result); }

    protected void OnProductSelect(string product)
    {
        // Called when user picks from the list
        var matchedProduct = Products.FirstOrDefault(e =>
            !string.IsNullOrWhiteSpace(e.ProductName) &&
            string.Equals(e.ProductName, product, StringComparison.OrdinalIgnoreCase));

        if (matchedProduct != null)
        {
            SaleItemDto.ProductId = matchedProduct.Id;
            SaleItemDto.ItemName = matchedProduct.ProductName;
        }
    }

    protected void OnProductTyped(string text)
    {
        
        SelectedProduct = text;
        SaleItemDto.ItemName = text;

        // Reset ProductId if no match
        var matchedProduct = Products.FirstOrDefault(e =>
            !string.IsNullOrWhiteSpace(e.ProductName) &&
            string.Equals(e.ProductName, text, StringComparison.OrdinalIgnoreCase));

        SaleItemDto.ProductId = matchedProduct?.Id;  // null if custom
        SelectedProductFromList = matchedProduct ?? new ProductDto();
        if(SelectedProductFromList is not null)
        {
            OnWholeSaleChanged(IsWholeSale);
        }
    }


    protected async Task OnBrandSelect(string brand) {
        if (string.IsNullOrWhiteSpace(brand) && ProductBrands == null)
            return;

        SelectedBrand = ProductBrands.FirstOrDefault(b =>
            !string.IsNullOrWhiteSpace(b.BrandName) &&
            string.Equals(b.BrandName, brand, StringComparison.OrdinalIgnoreCase)
        );
        if(SelectedBrand != null)
        {
            await LoadBrandProducts();
            SelectedProduct = string.Empty;
            SaleItemDto.ProductId = null;
            SaleItemDto.ItemName = string.Empty;
            StateHasChanged();
        }
    }

    protected void SaveItem()
    {
        if(SaleItemDto.ProductId != null && IsWholeSale)
        {
            SaleItemDto.ProductPricingOption = ProductPricingOption.WholeSale;
        }
        else if (SaleItemDto.ProductId !=null && !IsWholeSale)
        {
            SaleItemDto.ProductPricingOption = ProductPricingOption.Retail;
        }
        else
        {
            SaleItemDto.ProductPricingOption = ProductPricingOption.Override;
        }
        SaleItemDto.DataList = Paints;
        MudDialog.Close(DialogResult.Ok(SaleItemDto));
        
    }
    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected bool ShouldShowSaleType()
    {
        return SaleItemDto.PaintCategory == PaintCategory.Solid
               && SaleItemDto.ProductId != null;
    }
    protected void OnWholeSaleChanged(bool value)
    {
        IsWholeSale = value;

        if (SelectedProductFromList == null)
            return;

        if (IsWholeSale)
        {
            SaleItemDto.ItemPrice = SelectedProductFromList.WholesalePrice.GetValueOrDefault();  // adjust property name
        }
        else
        {
            SaleItemDto.ItemPrice = SelectedProductFromList.RetailPrice.GetValueOrDefault();    // adjust property name
        }

        StateHasChanged(); // force UI update
    }


    protected async Task AddPaintIncluded()
    {
        var parameter = new DialogParameters
        {
            {"Branch", Branch }
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        // Show the dialog with a proper title
        var dialog = await DialogService.ShowAsync<AddPaintsIncluded>(
            "Add Paint Included",parameter, options
        );

        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is InvolvePaintsDto newPaint)
        {
            bool exists = Paints.Any(p =>
                p.BrandId == newPaint.BrandId &&
                p.ProductId == newPaint.ProductId);

            if (!exists)
            {
                newPaint.Id = Paints.Any() ? Paints.Max(p => p.Id) + 1 : 1;

                Paints.Add(newPaint);
                StateHasChanged();
                Snackbar.Add("Item is Added", Severity.Success);
            }
            else
            {
                Snackbar.Add("This item is already included!", Severity.Warning);
            }
        }

    }
}
