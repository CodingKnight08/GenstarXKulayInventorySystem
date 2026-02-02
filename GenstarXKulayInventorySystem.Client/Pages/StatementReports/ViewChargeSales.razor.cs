using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace GenstarXKulayInventorySystem.Client.Pages.StatementReports;

public partial class ViewChargeSales
{
    [Parameter] public int ClientId { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private IDialogService DialogService        { get; set; } = default!;
    [Inject] private ILogger<ViewChargeSales> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    protected ClientDto? Client { get; set; } = new ClientDto();

    protected bool IsLoading { get; set; } = false;
    protected bool IsEdit { get; set; } = false;
    protected decimal TotalBalance { get; set; }
    private List<BreadcrumbItem> _items =
     [
        new("Statements", href: "/statements"),
        new("View Statement", href: null, disabled: true)
     ];
    protected override async Task OnInitializedAsync()
    {
        await LoadClientData();
    }

    protected async Task LoadClientData()
    { 
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/statementreport/{ClientId}");
            if (response.IsSuccessStatusCode)
            {
                var client = await response.Content.ReadFromJsonAsync<ClientDto>();
                if (client != null)
                {
                    Client = client;
                    TotalBalance =
                    (Client.DailySales?.Sum(x => x.TotalAmount ?? 0m) ?? 0m)
                    + (Client.RemainingChargeBalance ?? 0m);


                }
            }
            else
            {
                Logger.LogError("Failed to load client data. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading client data");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnRemainingChargeChanged(decimal? value)
    {
        if (Client == null)
            return;

        Client.RemainingChargeBalance = value ?? 0m;

        // Recompute total balance
        TotalBalance =
            (Client.DailySales?.Sum(x => x.TotalAmount ?? 0m) ?? 0m)
            + Client.RemainingChargeBalance.Value;

        StateHasChanged();
    }

    private async Task SelectMonth()
    {
        var parameters = new DialogParameters
        {
          { "ClientId", ClientId }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<MonthSelector>("Generate SOA", parameters, options);
        var result = await dialog.Result;
        if (result?.Data is bool success)
        {
            await LoadClientData();
        }
    }

    private async Task UpdateRemainingCharge()
    {
        if(Client == null) return;
        IsEdit = false;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/statementreport/{Client.Id}/remaining-balance", new
            {
                ClientId = Client.Id,
                RemainingChargeBalance = Client.RemainingChargeBalance
            });
            if (response.IsSuccessStatusCode)
            {
                // Optionally show a success message
            }
            else
            {
                Logger.LogError("Failed to update remaining charge. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating remaining charge");
        }
        finally
        {
            SnackBar.Add("Remaining charge updated successfully.", Severity.Success);
        }

    }
    private void ViewSale(int dailySaleId)
    {
        Navigation.NavigateTo($"/sales/view/{dailySaleId}");
    }

    private async Task SetAsPaid(int dailySaleId)
    {
       var dialog = await DialogService.ShowMessageBox(
            "Confirm Payment",
            "Are you sure you want to mark this sale as paid?",
            yesText: "Yes", noText: "No");
        if (dialog == true)
        {
            try
            {
                var response = await HttpClient.PutAsync($"api/sales/setpaid/{dailySaleId}", null);
                if (response.IsSuccessStatusCode)
                {
                    await LoadClientData();
                    SnackBar.Add("Sale marked as paid successfully.", Severity.Success);
                }
                else
                {
                    Logger.LogError("Failed to mark sale as paid. Status Code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error marking sale as paid");
            }
        }
    }
 

}
