using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems;

public partial class EditSaleItems
{
    [Parameter] public List<SaleItemDto> EditableSaleItems { get; set; } = new List<SaleItemDto>();
    [Parameter] public EventCallback<List<SaleItemDto>> OnSaleItemsChanged { get; set; }   

    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    protected List<SaleItemDto> SaleItems { get; set; } = new();

    protected override void OnParametersSet()
    {
        SaleItems = EditableSaleItems
            .Select(item => new SaleItemDto
            {
                Id = item.Id,
                ItemName = item.ItemName,
                Quantity = item.Quantity,
                ItemPrice = item.ItemPrice,
                // copy all other props here
                IsDeleted = item.IsDeleted
            })
            .Where(item => !item.IsDeleted)
            .ToList();
    }

    protected async Task RemoveSale(int id)
    {
        var parameters = new DialogParameters
        {
            { "ContentText", "Are you sure you want to remove this item?" },
            { "ButtonText", "Yes" },
            { "Color", Color.Error }
        };

        var options = new DialogOptions { CloseOnEscapeKey = true };

        var dialog = await DialogService.ShowAsync<DeleteSaleItems>("Confirm Removal", parameters, options);
        var result = await dialog.Result;

        if (!result.Canceled) // user clicked "Yes"
        {
            var toBeUpdatedItem = SaleItems.FirstOrDefault(e => e.Id == id);
            if (toBeUpdatedItem is not null)
            {
                toBeUpdatedItem.IsDeleted = true;

                // refresh UI list
                SaleItems = SaleItems.Where(e => !e.IsDeleted).ToList();

                // notify parent
                await OnSaleItemsChanged.InvokeAsync(SaleItems);

                StateHasChanged();
            }
        }
    }
}


