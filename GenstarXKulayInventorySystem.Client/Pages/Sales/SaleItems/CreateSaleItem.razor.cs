using GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems.PaintIncluded;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
    [Inject] protected UserState UserState { get; set; } = default!;
    protected SaleItemDto SaleItemDto { get; set; } = new SaleItemDto();
    protected List<ProductBrandDto> ProductBrands { get; set; } = new List<ProductBrandDto>();
    protected List<BranchProductDto> Products { get; set; } = new List<BranchProductDto>();
    protected BranchProductDto? SelectedProductFromList { get; set; } = new BranchProductDto();
    protected List<InvolvePaintsDto> Paints { get; set; } = new List<InvolvePaintsDto>();
    protected string BrandName { get; set; } = string.Empty;
    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected string SelectedProduct { get; set; } = string.Empty;
    protected bool IsLoading { get; set; } = false;
    protected bool IsProductLoading { get; set; } = false;
    protected bool IsWholeSale { get; set; } = false;
    protected bool IsRepack { get; set; } = false;
    protected bool OverridePrice { get; set; } = false;
    protected decimal PriceItem { get; set; } = 0;

    protected bool IsValid => !string.IsNullOrWhiteSpace(SaleItemDto.ItemName) && PriceItem > 0 && SaleItemDto.Quantity > 0;
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
                Products = new List<BranchProductDto>();
                Snackbar.Add("Products for the selected brand is low.", Severity.Warning);
                return;
            }
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>();
            Products = products ?? new List<BranchProductDto>();

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

    protected async Task OnProductSelectDto(BranchProductDto product)
    {
        if (product is null)
        {
            SelectedProductFromList = null;
            SaleItemDto.BranchProductId = null;
            SaleItemDto.ItemName = string.Empty;
            return;
        }

        SelectedProductFromList = product;
        SelectedProduct = product.MasterProduct?.ProductName ?? string.Empty;
        SaleItemDto.BranchProductId = product.Id;
        SaleItemDto.ItemName = product.MasterProduct?.ProductName ?? string.Empty;
        SaleItemDto.UnitMeasurement = product.ProductMesurementOption.GetValueOrDefault();

        await OnWholeSaleChanged(IsWholeSale);
    }

    protected void OnBrandTyped(string text)
    {
        SelectedBrand = ProductBrands.FirstOrDefault(b =>
            !string.IsNullOrWhiteSpace(b.BrandName) &&
            string.Equals(b.BrandName, text, StringComparison.OrdinalIgnoreCase));
        if (SelectedBrand != null)
        {
            BrandName = SelectedBrand.BrandName ?? string.Empty;
            _ = OnBrandSelect(SelectedBrand);
        }
        else
        {
            BrandName = text;
            SelectedBrand = null;
            Products = new List<BranchProductDto>();
            SelectedProduct = string.Empty;
            SaleItemDto.BranchProductId = null;
            SaleItemDto.ItemName = string.Empty;
        }
    }
    protected void OnProductTyped(string text)
    {
        SelectedProduct = text;
        SaleItemDto.ItemName = text;
        SaleItemDto.BranchProductId = null;
        SelectedProductFromList = null;
        
    }

   protected void OnPaintCategoryChange(PaintCategory paintType)
    {
        SaleItemDto.PaintCategory = paintType;
        SaleItemDto.BranchProductId = null;
        SaleItemDto.ItemName = string.Empty;
        SelectedProduct = string.Empty;
        SaleItemDto.ItemPrice = 0;
        SelectedBrand = new ProductBrandDto();
        Products = new List<BranchProductDto>();
        if(paintType == PaintCategory.Repack)
        {
            IsRepack = true;
        }

    }

    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        if (brand is null)
        {
            SelectedBrand = null;
            Products.Clear();
            SelectedProductFromList = null;
            SaleItemDto.BranchProductId = null;
            SaleItemDto.ItemName = string.Empty;
            return;
        }

        SelectedBrand = brand;
        BrandName = brand.BrandName ?? string.Empty;

        // Clear old selections
        SelectedProductFromList = null;
        SaleItemDto.BranchProductId = null;
        SaleItemDto.ItemName = string.Empty;

        await LoadBrandProducts();
    }


    protected void SaveItem()
    {
        if(SaleItemDto.PaintCategory != PaintCategory.Mix)
        {
            ComputeNotBelowWholeSale();
        }
        else
        {
            SaleItemDto.HasDiscount = SaleItemDto.TotalPrice - Paints.Sum(e => e.ProductCost) <= 0;
        }
        SaleItemDto.ItemPrice = PriceItem;
        if (SaleItemDto.BranchProductId != null && IsWholeSale && SaleItemDto.ItemPrice == SelectedProductFromList?.WholeSalePrice.GetValueOrDefault())
        {
            SaleItemDto.ProductPricingOption = ProductPricingOption.WholeSale;
        }
        else if (SaleItemDto.BranchProductId !=null && !IsWholeSale && SaleItemDto.ItemPrice == SelectedProductFromList?.RetailPrice)
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
               && SelectedProductFromList?.WholeSalePrice.HasValue == true
               && SelectedProductFromList.WholeSalePrice.Value > 0;

    }
    protected async Task OnWholeSaleChanged(bool value)
    {
        IsWholeSale = value;

        if (SelectedProductFromList == null)
            return;

        if (IsWholeSale)
            PriceItem = SelectedProductFromList.WholeSalePrice.GetValueOrDefault();
        else
            PriceItem = SelectedProductFromList.RetailPrice.GetValueOrDefault();

        RecalculateTotalPrice();
        await Task.CompletedTask;
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
   
    protected async Task Trial(decimal price)
    {
        PriceItem = price;
        RecalculateTotalPrice();
        await Task.CompletedTask;
    }

    protected void ComputeNotBelowWholeSale()
    {
        if(SaleItemDto.BranchProductId != null && SelectedProductFromList != null && PriceItem > 0)
        {
            decimal basePrice;
            if (SelectedProductFromList.WholeSalePrice.HasValue && SelectedProductFromList.WholeSalePrice.Value > 0)
            {
                basePrice = SelectedProductFromList.WholeSalePrice ?? 0 * SaleItemDto.Size ?? 1 * SaleItemDto.Quantity;
                decimal retailBasePrice = SelectedProductFromList.RetailPrice * SaleItemDto.Size ?? 1 * SaleItemDto.Quantity;
                if (SaleItemDto.TotalPrice < basePrice)
                {
                    SaleItemDto.HasDiscount = true;
                }
                else
                {
                    SaleItemDto.HasDiscount = false;
                }
            }

            else
            {
                basePrice = SelectedProductFromList.CostPrice * SaleItemDto.Size ?? 1 * SaleItemDto.Quantity;
                if(SaleItemDto.TotalPrice < basePrice)
                {
                    SaleItemDto.HasDiscount = true;
                }
                else
                {
                    SaleItemDto.HasDiscount = false;
                }
            }
        }
    }
    protected void OnSizeChanged(decimal? newSize)
    {
        SaleItemDto.Size = newSize;
        RecalculateTotalPrice();
    }

    private void RecalculateTotalPrice()
    {
        var size = SaleItemDto.Size ?? 1;
        var qty = SaleItemDto.Quantity > 0 ? SaleItemDto.Quantity : 1;

        SaleItemDto.TotalPrice = PriceItem * size * qty;
        StateHasChanged();
    }
    protected void OnQuantityChanged(decimal newQty)
    {
        SaleItemDto.Quantity = newQty;
        RecalculateTotalPrice();
    }
    protected void RemovePaintIncluded(InvolvePaintsDto item)
    {
        Paints.Remove(item);

        RecalculateMixDiscount();
        StateHasChanged();
    }
    private void RecalculateMixDiscount()
    {
        var paintsTotal = Paints.Sum(p => p.ProductCost);

        SaleItemDto.HasDiscount = SaleItemDto.TotalPrice - paintsTotal <= 0;
    }

}
