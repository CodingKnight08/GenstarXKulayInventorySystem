
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

    protected override async Task OnParametersSetAsync()
    {
        await LoadPurchaseOrderItemsAsync();
    }

    private async Task LoadPurchaseOrderItemsAsync()
    {
        IsLoading = true;

        try
        {
            var status = IsRecieved ? "received" : "unreceived";

            var response = await HttpClient.GetAsync(
                $"api/purchaseorderitem/{PurchaseOrderId}/items/{status}");

            response.EnsureSuccessStatusCode();

            PurchaseOrderItems =
                await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>()
                ?? new List<PurchaseOrderItemDto>();

            decimal total = PurchaseOrderItems.Sum(x =>
                (x.ItemAmount ?? 0) * x.ItemQuantity);

            await OnTotalCalculated.InvokeAsync(total);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading purchase order items.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
