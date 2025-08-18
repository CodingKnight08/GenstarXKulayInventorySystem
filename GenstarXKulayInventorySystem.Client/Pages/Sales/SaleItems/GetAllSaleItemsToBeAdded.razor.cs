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
    
    protected List<SaleItemDto> SaleItemsToBeAdded { get; set; } = new List<SaleItemDto>();
    protected bool IsLoading { get; set; } = false;

    protected async Task AddSaleItem()
    {
        var dialog = await Dialog.ShowAsync<CreateSaleItem>("Add Sale Item",
            new DialogParameters
            {
                { "Branch", Branch}
            },
            new DialogOptions
            {
                MaxWidth = MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                BackdropClick = false
            }
        );

        if(dialog != null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled) {
                var saleItem = result.Data as SaleItemDto;
                if (saleItem != null)
                {
                    // ✅ Add logic here, e.g.
                    SaleItemsToBeAdded.Add(saleItem);
                    StateHasChanged();
                    await OnSaleItemsChanged.InvokeAsync(SaleItemsToBeAdded);
                }
            }

        }
    }

    
}
