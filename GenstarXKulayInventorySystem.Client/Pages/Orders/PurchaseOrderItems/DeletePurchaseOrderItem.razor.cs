using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrderItems;

public partial class DeletePurchaseOrderItem
{
    [Parameter] public PurchaseOrderItemDto PurchaseOrderItem { get; set; } = default!;
    [Inject] protected ILogger<DeletePurchaseOrderItem> Logger { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;

    protected async Task DeletePurchaseOrderItemAsync()
    {
        try
        {
            var response = await HttpClient.DeleteAsync($"api/purchaseorderitem/{PurchaseOrderItem.Id}");
            if (response.IsSuccessStatusCode)
            {

                Snackbar.Add("Purchase order item deleted successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(PurchaseOrderItem));
                Logger.LogInformation("Purchase order item deleted successfully.");
            }
            else
            {
                Logger.LogError("Failed to delete purchase order item. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while deleting the purchase order item.");
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
        Logger.LogInformation("Delete operation cancelled.");
    }
}
