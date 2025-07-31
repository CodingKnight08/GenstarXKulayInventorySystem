using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrders;

public partial class GetAllPurchaseOrders
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllPurchaseOrders> Logger { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    protected List<PurchaseOrderDto> PurchaseOrders { get; set; } = new();
    protected bool IsLoading { get; set; } = true;


    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadPurchaseOrders();
        await Task.Delay(2000);
        IsLoading = false;
        
    }


    protected async Task LoadPurchaseOrders()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/purchaseorder/all");
            response.EnsureSuccessStatusCode();
            var orders = await response.Content.ReadFromJsonAsync<List<PurchaseOrderDto>>();
            PurchaseOrders = orders ?? new List<PurchaseOrderDto>();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading purchase orders: {ex.Message}", Severity.Error);
        }
        
    }


    protected async Task CreatePurchaseOrder()
    {
        try
        {
            var options = new DialogOptions() { FullScreen = true, CloseButton = true, MaxWidth = MaxWidth.False };

            var dialog = await DialogService.ShowAsync<CreatePurchaseOrders>("Create Purchase Order", options);

            if (dialog is not null)
            {
                var result = await dialog.Result;

                if (result is not null && !result.Canceled)
                {
                    await LoadPurchaseOrders();
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Creating purchase order dialog error, {ex}", Severity.Error);
        }
    }


    protected void ViewPurchaseOrder(int purchaseOrderId)
    {
        NavigationManager.NavigateTo($"/purchase-order/view/{purchaseOrderId}");
    }

}
