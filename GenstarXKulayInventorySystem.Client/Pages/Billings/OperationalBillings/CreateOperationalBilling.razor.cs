using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.OperationalBillings;

public partial class CreateOperationalBilling
{
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    protected BillingDto Billing { get; set; } = new();
    protected string? ErrorMessage { get; set; }
    protected DateTime MinDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
    
    protected override async Task OnInitializedAsync()
    {
        Billing.Category = BillingCategory.Logistics;
        Billing.DateOfBilling = DateTime.Today;
       
        await Task.CompletedTask;
    }
    protected async Task SubmitAsync()
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/billings/operational", Billing);
            if (response.IsSuccessStatusCode)
            {
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                ErrorMessage = "Failed to create billing. Please try again.";
                Snackbar.Add(ErrorMessage, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            Snackbar.Add(ErrorMessage, Severity.Error);
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

    protected bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Billing.BillingName)
        && Billing.Amount != 0;
    }
}
