using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrderItems;

public partial class GetAllToBeAddedPurchaseOrderItems
{
    [Parameter] public EventCallback<List<PurchaseOrderItemDto>> OnPurchaseOrderItemsChanged { get; set; }
    [Parameter] public PurchaseShipToOption Branch { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllToBeAddedPurchaseOrderItems> Logger { get; set; } = default!;

    protected List<PurchaseOrderItemDto> PurchaseOrderItems { get; set; } = new();


    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    protected async Task NotifyParent()
    {
        await OnPurchaseOrderItemsChanged.InvokeAsync(PurchaseOrderItems);
    }
    protected async Task AddPurchaseOrderItem()
    {
        try
        {
            BranchOption branch = Branch switch
            {
                PurchaseShipToOption.GeneralSantosCity => BranchOption.GeneralSantosCity,
                PurchaseShipToOption.Polomolok => BranchOption.Polomolok,
                PurchaseShipToOption.Warehouse => BranchOption.Warehouse,
                _ => throw new ArgumentOutOfRangeException()
            };

            var dialog = await DialogService.ShowAsync<CreatePurchaseOrderItem>("", new DialogParameters
            {
                {"Branch", branch}
            });
            if (dialog is not null)
            {
                var result = await dialog.Result;
                if (result is not null && !result.Canceled)
                {
                    var purchaseOrderItem = result.Data as PurchaseOrderItemDto;
                    if (purchaseOrderItem is not null)
                    {
                        bool isDuplicate = PurchaseOrderItems.Any(p =>
                            p.ProductBrandId == purchaseOrderItem.ProductBrandId &&
                            p.ProductId == purchaseOrderItem.ProductId);

                        if (!isDuplicate)
                        {
                            PurchaseOrderItems.Add(purchaseOrderItem);
                            await NotifyParent();
                            Snackbar.Add("Purchase Order Item added successfully.", Severity.Success);
                            StateHasChanged();
                        }
                        else
                        {
                            Snackbar.Add("Purchase Order Item already exists.", Severity.Warning);
                        }
                    }

                }
            }
        }
        catch(Exception ex)
        {
            Logger.LogError($"Adding PO item dialog error, {ex}", Severity.Error);
        }
    }

    protected async Task RemovePurchaseItem(int brandId, int productId)
    {
        var itemToRemove =PurchaseOrderItems.FirstOrDefault(item => item.ProductBrandId == brandId && item.ProductId == productId);
        if (itemToRemove != null)
        {
            PurchaseOrderItems.Remove(itemToRemove);
            await NotifyParent();
        }
        StateHasChanged();
    }
}
