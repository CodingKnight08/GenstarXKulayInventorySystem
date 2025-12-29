using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace GenstarXKulayInventorySystem.Client.Pages.StatementReports;

public partial class ViewChargeSales
{
    [Parameter] public int ClientId { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;

    [Inject] private ILogger<ViewChargeSales> Logger { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;
    protected ClientDto? Client { get; set; } = new ClientDto();

    protected bool IsLoading { get; set; } = false;
    protected bool IsEdit { get; set; } = false;
    protected decimal TotalBalance { get; set; }

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

    private void OnRemainingChargeChanged(decimal? value)
    {
        if (Client == null)
            return;

        Client.RemainingChargeBalance = value ?? 0m;

        // Recompute total balance
        TotalBalance =
            (Client.DailySales?.Sum(x => x.TotalAmount ?? 0m) ?? 0m)
            + Client.RemainingChargeBalance.Value;


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

    private async Task GeneratePdf()
    {
        if (Client == null) return;

        var pdfBytes = await HttpClient.GetByteArrayAsync($"api/statementreport/generate/{Client.Id}");
        await DownloadPdf(pdfBytes, $"Statement_{Client.ClientName}.pdf");
    }
    private async Task DownloadPdf(byte[] pdfBytes, string fileName)
    {
        using var streamRef = new DotNetStreamReference(stream: new MemoryStream(pdfBytes));
        await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

}
