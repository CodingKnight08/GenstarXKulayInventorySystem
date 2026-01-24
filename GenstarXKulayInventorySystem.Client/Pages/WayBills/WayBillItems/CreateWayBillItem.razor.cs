using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills.WayBillItems;

public partial class CreateWayBillItem
{
    [Parameter] public BranchOption Branch { get; set; }
    [Parameter] public int BrandId { get; set; }
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    [Inject] private ILogger<CreateWayBillItem> Logger { get; set; } = default!;

    protected WayBillItemsDto WayBillItem { get; set; } = new WayBillItemsDto();
    protected List<BranchProductDto> BranchProducts { get; set; } = new List<BranchProductDto>();

    protected BranchProductDto? SelectedProduct { get; set; } = new BranchProductDto();
    protected bool IsLoading { get; set; } = false;
    protected bool IsValid => WayBillItem.BranchProductId > 0 && WayBillItem.Quantity > 0 ;

    protected override async Task OnParametersSetAsync()
    {
        await LoadBranchProducts();
    }

    protected async Task LoadBranchProducts()
    {
       IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/existing/products/{BrandId}/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var branchProducts = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>();
                if (branchProducts != null)
                {
                    BranchProducts = branchProducts;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading branch products.");
            SnackBar.Add("Error loading branch products.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }

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

    protected async Task OnProductSelect(BranchProductDto productDto)
    {
        if(productDto == null)
        {
            SelectedProduct = null;
            WayBillItem.BranchProductId = 0;
            WayBillItem.ItemPrice = 0;
            return;
        }
        SelectedProduct = productDto;
        WayBillItem.BranchProductId = productDto.Id;
        WayBillItem.BranchProduct = productDto;
        WayBillItem.ItemPrice = productDto.CostPrice ?? 0;
        RecalculateTotalPrice();
    }

    protected void SaveItem()
    {
        WayBillItem.ActualQuantity = WayBillItem.Quantity;
        
        MudDialog.Close(DialogResult.Ok(WayBillItem));
    }

    protected void Cancel() => MudDialog.Cancel();
    protected void OnQuantityChanged(decimal newQty)
    {
        WayBillItem.Quantity = newQty;
        WayBillItem.ActualQuantity = newQty;
        RecalculateTotalPrice();
    }

    protected void RecalculateTotalPrice()
    {
        WayBillItem.TotalPrice = WayBillItem.ItemPrice * WayBillItem.Quantity;
        StateHasChanged();
    }
}
