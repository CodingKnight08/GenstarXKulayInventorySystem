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

    protected async Task AddSaleItem()
    {
        var dialog = await Dialog.ShowAsync<CreateSaleItem>(
            "Add Sale Item",
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

        if (dialog == null)
            return;

        var result = await dialog.Result;

        if (result == null || result.Canceled)
            return;

        var saleItems = result.Data as List<SaleItemDto>;

        if (saleItems == null || !saleItems.Any())
            return;

        int addedCount = 0;
        int duplicateCount = 0;

        foreach (var saleItem in saleItems)
        {
            // Mix category is always allowed
            bool exists = saleItem.PaintCategory != PaintCategory.Mix &&
                          SaleItemsToBeAdded.Any(x =>
                              x.BranchProductId == saleItem.BranchProductId &&
                              string.Equals(x.ItemName, saleItem.ItemName, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                SaleItemsToBeAdded.Add(saleItem);
                addedCount++;
            }
            else
            {
                duplicateCount++;
            }
        }

        if (addedCount > 0)
        {
            await OnSaleItemsChanged.InvokeAsync(SaleItemsToBeAdded);
            StateHasChanged();

            SnackBar.Add($"{addedCount} item(s) added successfully!", Severity.Success);
        }

        if (duplicateCount > 0)
        {
            SnackBar.Add($"{duplicateCount} duplicate item(s) skipped.", Severity.Warning);
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
                StateHasChanged();
                await OnSaleItemsChanged.InvokeAsync(SaleItemsToBeAdded);
                SnackBar.Add($"'{item.ItemName}' removed successfully.", Severity.Info);
            }
        });
    }

    

}
