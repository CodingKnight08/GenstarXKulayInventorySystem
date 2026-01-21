using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills.WayBillItems;

public partial class GetAllWayBillItems
{
    [Parameter] public BranchOption Branch { get; set; }
    [Parameter] public int BrandId { get; set; }
    [Parameter] public EventCallback OnItemsChanged { get; set; }

    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private ILogger<GetAllWayBillItems> Logger { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    public List<WayBillItemsDto> WayBillItems { get; set; } = new List<WayBillItemsDto>();
    protected bool BrandIsChange { get; set; } = false;
    protected bool HasBrand { get; set; } = false;
    protected int InitialBrandId { get; set; }
    private int _previousBrandId;
    protected override void OnParametersSet()
    {
        HasBrand = BrandId > 0;
        if (_previousBrandId != 0 && BrandId != _previousBrandId)
        {
            ClearWayBillItems();
        }

        _previousBrandId = BrandId;
    }
    private void ClearWayBillItems()
    {
        WayBillItems.Clear();
        BrandIsChange = true;
        SnackBar.Add("Brand changed. Way bill items were cleared.", Severity.Warning);
    }

    protected async Task AddWayBillItems()
    {
        var dialogParameters = new DialogParameters
        {
            { "Branch", Branch },
            { "BrandId", BrandId }
        };
        var options = new DialogOptions 
        { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, BackdropClick = false };
        var dialog = await DialogService.ShowAsync<CreateWayBillItem>("Add Way Bill Items", dialogParameters, options);
        var result = await dialog.Result;

        if (!result.Canceled && result.Data is WayBillItemsDto item)
        {
            // Check if the product already exists in the list
            if (WayBillItems.Any(x => x.BranchProductId == item.BranchProductId))
            {
                SnackBar.Add("This product is already added.", Severity.Warning);
                return;
            }
            SnackBar.Add("Way bill item added successfully.", Severity.Success);
            WayBillItems.Add(item);
            StateHasChanged();
            await OnItemsChanged.InvokeAsync();
        }
    }

    protected async Task RemoveItem(WayBillItemsDto item)
    {
        if (WayBillItems.Contains(item))
        {
            WayBillItems.Remove(item);
            SnackBar.Add("Item removed successfully.", Severity.Info);
            StateHasChanged();
            await OnItemsChanged.InvokeAsync();
        }
    }

}
