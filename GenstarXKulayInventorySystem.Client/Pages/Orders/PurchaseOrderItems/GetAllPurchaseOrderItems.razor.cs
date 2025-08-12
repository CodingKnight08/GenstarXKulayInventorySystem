using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrderItems;

public partial class GetAllPurchaseOrderItems
{
    [Parameter] public int PurchaseOrderId { get; set; }
    [Parameter] public bool IsRecieved { get; set; }
    [Parameter] public EventCallback<decimal> OnTotalCalculated { get; set; }


    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllPurchaseOrderItems> Logger { get; set; } = default!;

    protected List<PurchaseOrderItemDto> PurchaseOrderItems { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected override async Task OnInitializedAsync()
    {
       IsLoading = true;
        try
        {
            if (IsRecieved)
            {
                await LoadRecievedPurchaseOrderItemsAsync();
            }
            else
            {
                await LoadUnrecievedPurchaseOrderItemsAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading purchase order items: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task LoadRecievedPurchaseOrderItemsAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/purchaseorderitem/{PurchaseOrderId}/items/received");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>();
                PurchaseOrderItems = items ?? new List<PurchaseOrderItemDto>();
                decimal total = PurchaseOrderItems.Sum(x => (x.ItemAmount ?? 0) * x.ItemQuantity);
                await OnTotalCalculated.InvokeAsync(total);

            }
            else
            {
                Logger.LogError($"Failed to load received purchase order items. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading received purchase order items: {ex.Message}");
        }
    }

    protected async Task LoadUnrecievedPurchaseOrderItemsAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/purchaseorderitem/{PurchaseOrderId}/items/unreceived");
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>();
                PurchaseOrderItems = items ?? new List<PurchaseOrderItemDto>();
                decimal total = PurchaseOrderItems.Sum(x => (x.ItemAmount ?? 0) * x.ItemQuantity);
                await OnTotalCalculated.InvokeAsync(total);

            }
            else
            {
                Logger.LogError($"Failed to load unreceived purchase order items. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading unreceived purchase order items: {ex.Message}");
        }
    }
}
