using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.PurchaseOrderBillings;

public partial class DeletePurchaseOrderBilling
{
    [Parameter] public PurchaseOrderBillingDto PurchaseOrderBilling { get; set; } = new PurchaseOrderBillingDto();
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] protected ILogger<DeletePurchaseOrderBilling> Logger { get; set; } = default!;

    protected bool IsLoading { get; set; } = false;

    protected async Task DeletePurchaseOrderBillingAsync()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/billings/purchase-orders/delete/{PurchaseOrderBilling.Id}");
            if (response.IsSuccessStatusCode)
            {
                Dialog.Close(DialogResult.Ok(true));
            }
        }
        catch(Exception ex)
        {
            Logger.LogError($"Error occured, {ex.Message}");
        }
        IsLoading = false;
    }

    protected void Cancel()
    {
        Dialog.Cancel();
    }
}
