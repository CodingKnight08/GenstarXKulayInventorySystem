using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems;

public partial class GetAllSaleItemsToBeAdded
{
    [Parameter] public BranchOption Branch { get; set; }
    [Parameter] public EventCallback<List<SaleItemDto>> OnSaleItemsChanged { get; set; }
    [Inject] protected IDialogService Dialog { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    
    protected List<SaleItemDto> SaleItemsToBeAdded { get; set; } = new List<SaleItemDto>();
    protected bool IsLoading { get; set; } = false;
    protected decimal TotalAmount { get; set; } = 0;
    protected async Task AddSaleItem()
    {
        var dialog = await Dialog.ShowAsync<CreateSaleItem>("Add Sale Item",
            new DialogParameters
            {
            { "Branch", Branch }
            },
            new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                BackdropClick = false
            }
        );

        if (dialog != null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled)
            {
                var saleItem = result.Data as SaleItemDto;
                if (saleItem != null)
                {
                    bool exists = SaleItemsToBeAdded.Any(x =>
                                x.PaintCategory != PaintCategory.Mix &&  // allow multiple Mix items
                                (
                                    (x.ProductId != null && x.ProductId == saleItem.ProductId) ||
                                    (!string.IsNullOrWhiteSpace(x.ItemName) &&
                                     string.Equals(x.ItemName, saleItem.ItemName, StringComparison.OrdinalIgnoreCase))
                                ));


                    if (!exists)
                    {
                        SaleItemsToBeAdded.Add(saleItem);
                       
                        await OnSaleItemsChanged.InvokeAsync(SaleItemsToBeAdded);
                        SnackBar.Add("Item has been added!", Severity.Success);
                    }
                    else
                    {
                        SnackBar.Add("Item already exists in the list!", Severity.Warning);
                    }
                    TotalAmount = SaleItemsToBeAdded.Sum(x => x.TotalPrice);
                    StateHasChanged();
                }
            }
        }
    }
    protected void RemoveSaleItem(SaleItemDto item)
    {
        if (item == null)
            return;

        // Confirm delete (optional)
        var confirmMessage = $"Are you sure you want to remove '{item.ItemName}'?";
        Dialog.ShowMessageBox("Confirm Delete", confirmMessage,
            yesText: "Yes", noText: "Cancel", options: new DialogOptions { CloseButton = true })
        .ContinueWith(async t =>
        {
            if (t.Result == true)
            {
                SaleItemsToBeAdded.Remove(item);
                TotalAmount = SaleItemsToBeAdded.Sum(x => x.TotalPrice);
                StateHasChanged();
                await OnSaleItemsChanged.InvokeAsync(SaleItemsToBeAdded);
                SnackBar.Add($"'{item.ItemName}' removed successfully.", Severity.Info);
            }
        });
    }

    

}
