using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.OperationalBillings;

public partial class GetAllOperationalBillings
{
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public ILogger<GetAllOperationalBillings> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    protected List<BillingDto> Billings { get; set; } = new();
    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadBillings();
    }

    protected async Task LoadBillings()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/billings/all/others");
            if (response.IsSuccessStatusCode)
            {
                Billings = await response.Content.ReadFromJsonAsync<List<BillingDto>>() ?? new List<BillingDto>();
                if (Billings.Count == 0)
                {
                    Snackbar.Add("No billings found.", Severity.Warning);
                }
            }
            else
            {
                Snackbar.Add("Failed to load billings.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading billings");
            Snackbar.Add("Failed to load billings.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task CreateBilling()
    {
        try
        {
            var dialog = await DialogService.ShowAsync<CreateOperationalBilling>("Create Billing",
                new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true});
            if (dialog is not null)
                {
                var result = await dialog.Result;
                if (result is not null && !result.Canceled && result.Data is bool success && success)
                {
                    Snackbar.Add("Billing created successfully.", Severity.Success);
                    await LoadBillings();
                    StateHasChanged();
                }
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error creating billing");
            Snackbar.Add("An error occured.", Severity.Error);
        }
    }

    protected async Task ViewBilling(int billingId)
    {
        try
        {
            var dialog = await DialogService.ShowAsync<ViewOperationalBilling>("View Billing",
                new DialogParameters { { "OperationalBillingId", billingId } },
                new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton=true });
            if (dialog is not null)
            {
                var result = await dialog.Result;
                if (result is not null && !result.Canceled && result.Data is bool success && success)
                {
                    
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing billing");
            Snackbar.Add("An error occured.", Severity.Error);
        }
    }

    protected async Task EditBilling(BillingDto billing)
    {
        try
        {
            var dialog = await DialogService.ShowAsync<EditOperationalBilling>("Edit Billing",
                new DialogParameters { { "Billing",billing } },
                new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseButton=true });
            if (dialog is not null)
            {
                var result = await dialog.Result;
                if (result is not null && !result.Canceled && result.Data is bool success && success)
                {
                    Snackbar.Add("Billing updated successfully.", Severity.Success);
                    await LoadBillings();
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing billing");
            Snackbar.Add("An error occured.", Severity.Error);
        }
    }

    protected async Task DeleteBilling(BillingDto billing)
    {
        try
        {
            var dialog = await DialogService.ShowAsync<DeleteOperationalBilling>("",
                new DialogParameters { { "Billing", billing } },
                new DialogOptions { MaxWidth = MaxWidth.Small, FullWidth = true, CloseButton=true });
            if (dialog is not null)
            {
                var result = await dialog.Result;
                if (result is not null && !result.Canceled && result.Data is bool success && success)
                {
                    Snackbar.Add("Billing deleted successfully.", Severity.Success);
                    await LoadBillings();
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting billing");
            Snackbar.Add("An error occured.", Severity.Error);
        }
    }
}
