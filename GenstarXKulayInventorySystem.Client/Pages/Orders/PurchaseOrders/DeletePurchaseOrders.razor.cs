using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrders;

public partial class DeletePurchaseOrders
{
    [Parameter] public PurchaseOrderDto PurchaseOrder { get; set; } = new PurchaseOrderDto();
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] public IMudDialogInstance DialogService { get; set; } = default!;

    protected async Task DeletePurchaseOrderAsync()
    {
        try
        {
            var response = await HttpClient.DeleteAsync($"api/purchaseorder/{PurchaseOrder.Id}");
            if (response.IsSuccessStatusCode)
            {
                
                DialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to delete purchase order. Please try again later.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting purchase order: {ex.Message}");
            Snackbar.Add("An error occurred while deleting the purchase order. Please try again later.", Severity.Error);
        }
    }

    protected void Cancel()
    {
        DialogService.Close(DialogResult.Cancel());
    }
}
