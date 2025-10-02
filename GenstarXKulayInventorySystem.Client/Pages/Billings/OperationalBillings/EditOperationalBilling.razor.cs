using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.OperationalBillings;

public partial class EditOperationalBilling
{
    [Parameter] public BillingDto Billing { get; set; } = new BillingDto();
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<EditOperationalBilling> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    protected DateTime MinDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
    protected bool IsUpdating { get; set; } = false;
    protected bool IsLoading { get; set; } = true;
    protected BillingDto EditableBilling { get; set; } = new BillingDto();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            IsLoading = true;

            if (Billing.Id == 0)
            {
                Snackbar.Add("Billing not found.", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
                return;
            }

            EditableBilling = new BillingDto
            {
                Id = Billing.Id,
                BillingName = Billing.BillingName,
                BillingNumber = Billing.BillingNumber,
                DateOfBilling = Billing.DateOfBilling,
                Amount = Billing.Amount,
                Branch = Billing.Branch,
                Category = Billing.Category,
                IsPaid = Billing.IsPaid,
                PaymentMethod = Billing.PaymentMethod,
                Remarks = Billing.Remarks,
                Discounted = Billing.Discounted,
                DiscountAmount = Billing.DiscountAmount,
                OperationsProvider = Billing.OperationsProvider
            };
        }
        finally
        {
            await Task.Delay(500);
            IsLoading = false;
        }
    }

    protected async Task UpdateOperationalBilling()
    {
        try
        {
            IsLoading = true;
            IsUpdating = true;

            // Apply edits to the real Billing object
            Billing.BillingName = EditableBilling.BillingName;
            Billing.BillingNumber = EditableBilling.BillingNumber;
            Billing.DateOfBilling = EditableBilling.DateOfBilling;
            Billing.Amount = EditableBilling.Amount;
            Billing.Branch = EditableBilling.Branch;
            Billing.Category = EditableBilling.Category;
            Billing.IsPaid = EditableBilling.IsPaid;
            Billing.PaymentMethod = EditableBilling.PaymentMethod;
            Billing.Remarks = EditableBilling.Remarks;
            Billing.Discounted = EditableBilling.Discounted;
            Billing.DiscountAmount = EditableBilling.DiscountAmount;

            var response = await HttpClient.PutAsJsonAsync($"api/billings/operational/{Billing.Id}", EditableBilling);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Billing updated successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                Snackbar.Add("Failed to update billing.", Severity.Error);
            }
        }
        finally
        {
            IsLoading = false;
            IsUpdating = false;
        }
    }


    protected void OnDateChanged(DateTime? date)
    {
        Billing.DateOfBilling = date ?? DateTime.Today;
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Billing.BillingName)
        && !string.IsNullOrWhiteSpace(Billing.BillingNumber)
        && Billing.Amount != 0;
    }
}
