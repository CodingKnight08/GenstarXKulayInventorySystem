using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.RecieveOrders;

public partial class ViewReceivePurchaseOrder
{
    [Parameter] public int Id { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewReceivePurchaseOrder> Logger { get; set; }= default!;

    protected bool IsLoading { get; set; } = false;
    protected PurchaseOrderDto PurchaseOrder { get; set; } = new PurchaseOrderDto();

    protected override async Task OnInitializedAsync()
    {
        await LoadReceivedOrders();
    }


    protected async Task LoadReceivedOrders()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/purchaseorder/{Id}");
            response.EnsureSuccessStatusCode();
            var purchaseOrder = await response.Content.ReadFromJsonAsync<PurchaseOrderDto>();
            if (purchaseOrder != null) {
                PurchaseOrder = purchaseOrder;
                
            }
            else
            {
                PurchaseOrder = new PurchaseOrderDto();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occured,{ex.Message}");
        }
        IsLoading = false;
    }

    protected List<BreadcrumbItem> _items =
   [
       new("Received Purchase Orders", href: "/received-order"),
        new("View Order", href: null, disabled: true)
   ];
    protected void DisplayEstimateAmount(decimal total)
    {
        PurchaseOrder.AssumeTotalAmount = total;
    }
}
