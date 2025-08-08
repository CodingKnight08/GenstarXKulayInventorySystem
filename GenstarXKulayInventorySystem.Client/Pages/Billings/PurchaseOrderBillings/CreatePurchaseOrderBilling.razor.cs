using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.PurchaseOrderBillings;

public partial class CreatePurchaseOrderBilling
{
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<CreatePurchaseOrderBilling> Logger { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    protected List<PurchaseOrderItemDto> RecievedPurchaseOrderItems { get; set; } = new();
    protected BillingDto Billing { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected DateTime MinDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);

    protected override async Task OnInitializedAsync()
    {
       IsLoading = true;
        await LoadRecievedPurchaseOrderItems();
        IsLoading = false;

    }

    protected async Task LoadRecievedPurchaseOrderItems()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/purchaseorderitem/all/recieved/purchaseorderitems");
            if (response.IsSuccessStatusCode)
            {
                RecievedPurchaseOrderItems = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>() ?? new List<PurchaseOrderItemDto>();
            }
            else
            {
                SnackBar.Add("Failed to load received purchase order items.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading received purchase order items");
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected void OnDateChanged(DateTime? date)
    {
        Billing.DateOfBilling = date ?? DateTime.Today;
    }
}
