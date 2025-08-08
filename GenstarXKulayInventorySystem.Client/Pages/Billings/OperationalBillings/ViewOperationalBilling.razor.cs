using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.OperationalBillings;

public partial class ViewOperationalBilling
{
    [Parameter] public int OperationalBillingId { get; set; }
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewOperationalBilling> Logger { get; set; } = default!;

    protected bool IsLoading { get; set; } = true;
    protected BillingDto Billing { get; set; } = new BillingDto();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            IsLoading = true;
            var response = await HttpClient.GetFromJsonAsync<BillingDto>($"api/billings/operational/{OperationalBillingId}");
            if(response is not null)
            {
                Billing = response;
            }
            else
            {
                Logger.LogWarning("No billing found with ID {OperationalBillingId}", OperationalBillingId);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching operational billing details");
            
        }
        finally
        {
            await Task.Delay(500);
            IsLoading = false;
        }
    }

    protected void CloseDialog()
    {
        MudDialog.Close(DialogResult.Ok(true));
    }
}
