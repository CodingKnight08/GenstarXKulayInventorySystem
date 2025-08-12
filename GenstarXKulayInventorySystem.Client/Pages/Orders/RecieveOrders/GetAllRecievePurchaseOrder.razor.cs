using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.RecieveOrders;

public partial class GetAllRecievePurchaseOrder
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<GetAllRecievePurchaseOrder> Logger { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    protected bool IsLoading { get; set; } = true;
    protected List<PurchaseOrderDto> PurchaseOrders { get; set; } = new List<PurchaseOrderDto>();
    protected override async Task OnInitializedAsync()
    {
        await LoadPurchaseOrder();
    }

    protected async Task LoadPurchaseOrder()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/purchaseorder/all/received");
            response.EnsureSuccessStatusCode();
            var purchaseOrders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
            PurchaseOrders = purchaseOrders ?? new List<PurchaseOrderDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occured, {ex}");
        }
        finally { 
            IsLoading = false;
        }
    }


    protected void ViewRecievePurchaseOrder(int purchaseOrderId)
    {
        NavigationManager.NavigateTo($"/receive-order/view/{purchaseOrderId}");
    }

    protected Color GetChipColor(PurchaseRecieveOption status)
    {
        return status switch
        {
            PurchaseRecieveOption.Pending => Color.Warning,
            PurchaseRecieveOption.PartialRecieve => Color.Info,
            PurchaseRecieveOption.RecieveAll => Color.Success,
            _ => Color.Default
        };
    }
} 
