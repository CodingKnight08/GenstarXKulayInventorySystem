using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrders;

public partial class ViewPurchaseOrder
{
    [Parameter] public int Id { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewPurchaseOrder> Logger { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;



    protected bool IsLoading { get; set; } = true;
    protected bool IsEdit { get; set; } = false;

    protected MudForm _form = default!;
    protected PurchaseOrderDto PurchaseOrder { get; set; } = new();

    protected List<BreadcrumbItem> _items =
    [
        new("Purchase Order", href: "/purchase-order"),
        new("View Order", href: null, disabled: true)
    ];

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadPurchaseOrder();
        IsLoading =false;

    }

    protected void DisplayEstimateAmount(decimal total)
    {
        PurchaseOrder.AssumeTotalAmount = total;
    }
    protected async Task LoadPurchaseOrder()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/purchaseorder/{Id}");
            response.EnsureSuccessStatusCode();
            var purchaseOrder = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();
            if (purchaseOrder is not null)
            {
                PurchaseOrder = purchaseOrder;
            }
            else
            {
                PurchaseOrder = null!;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading purchase order: {ex}");
        }
    }
    protected void ToggleEdit() { 
        IsEdit = !IsEdit;
    }

    protected void UpdatePurchaseOrderData(PurchaseOrderDto updatedDto)
    {
        PurchaseOrder = updatedDto;
        IsEdit = false;
        
         StateHasChanged();
    }

    protected async Task DeletePurchaseOrder()
    {
        try
        {
            var dialog = await DialogService.ShowAsync<DeletePurchaseOrders>("Delete Purchase Order", new DialogParameters
            {
                { "PurchaseOrder", PurchaseOrder }
            });
            if(dialog is not null)
            {
                var result = await dialog.Result;
                if(result is not null && !result.Canceled)
                {
                    IsLoading = true;
                    StateHasChanged(); 
                    await Task.Delay(10000); 
                    NavigationManager.NavigateTo("/purchase-order");


                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error deleting purchase order: {ex}");
            
        }
    }
}
