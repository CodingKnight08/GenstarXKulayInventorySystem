using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrderItems;

public partial class EditPurchaseOrderItems
{
    [Parameter] public int PurchaseOrderId { get; set; } 
    
    [Parameter] public EventCallback<List<PurchaseOrderItemDto>> OnUpdatePurchaseOrderItems { get; set; }
    [Inject] protected ILogger<EditPurchaseOrderItems> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    protected List<PurchaseOrderItemDto> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItemDto>();
    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadPurchaseOrderItems();
        
    }  
    
    protected async Task LoadPurchaseOrderItems()
    {
        IsLoading   = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/purchaseorderitem/{PurchaseOrderId}/items/unreceived");
            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>();
                if (items is not null)
                {
                    PurchaseOrderItems = items;
                    await OnUpdatePurchaseOrderItems.InvokeAsync(PurchaseOrderItems);
                }
                else
                {
                    PurchaseOrderItems.Clear();
                }
            }
            else
            {
                Snackbar.Add("Failed to load purchase order items.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading purchase order items: {ex}");
            Snackbar.Add("Failed to load purchase order items.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }



    protected async Task RemovePurchaseOrderItem(PurchaseOrderItemDto purchaseItem)
    {
        var dialog = await DialogService.ShowAsync<DeletePurchaseOrderItem>(
            "Delete Purchase Order Item",
            new DialogParameters { ["PurchaseOrderItem"] = purchaseItem });

        if (dialog is not null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is PurchaseOrderItemDto deletedItem)
            {
                PurchaseOrderItems.RemoveAll(x => x.Id == deletedItem.Id);
                
                await LoadPurchaseOrderItems();
                StateHasChanged();
            }
        }
    }

}
