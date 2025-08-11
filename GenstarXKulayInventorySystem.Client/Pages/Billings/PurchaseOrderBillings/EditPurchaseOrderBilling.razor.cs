using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.PurchaseOrderBillings;

public partial class EditPurchaseOrderBilling
{
    [Parameter] public PurchaseOrderBillingDto PurchaseOrderBilling { get; set; } = new PurchaseOrderBillingDto();
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<EditPurchaseOrderBilling> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;

    protected bool IsLoading { get; set; } = false;

    protected async Task UpdatePurchaseOrderBilling()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/billings/purchase-orders/{PurchaseOrderBilling.Id}", PurchaseOrderBilling);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Purchase order billing is updated successfully!", Severity.Success);
                Dialog.Close(DialogResult.Ok(true));
            }

        }
        catch (Exception ex) {
            Logger.LogError($"Error {ex}", Severity.Error);
        }
    }
    protected string GetEnumDescription(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                              .Cast<DescriptionAttribute>()
                              .FirstOrDefault();
        return attribute?.Description ?? value.ToString();
    }

    protected void Cancel()
    {
        Dialog.Cancel();
    }
}
