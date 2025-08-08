using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.OperationalBillings;

public partial class DeleteOperationalBilling
{
    [Parameter] public BillingDto Billing { get; set; } = new();
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;

    protected async Task DeleteBilling()
    {
        try
        {
            var response = await HttpClient.DeleteAsync($"api/billings/operational/delete/{Billing.Id}");
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Billing deleted successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to delete billing.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error deleting billing: {ex.Message}", Severity.Error);
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
