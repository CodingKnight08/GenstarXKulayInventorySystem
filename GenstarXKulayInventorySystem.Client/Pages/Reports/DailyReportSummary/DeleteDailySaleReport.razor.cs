using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class DeleteDailySaleReport
{
    [Parameter] public int DailySaleReportId { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    protected async Task DeleteDailySaleReportAsync()
    {
        var response = await HttpClient.DeleteAsync($"api/dailysalereport/{DailySaleReportId}");
        if (response.IsSuccessStatusCode)
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            // Handle error (e.g., show a message to the user)
            MudDialog.Close(DialogResult.Cancel());
        }
    }

    protected void Cancel() => MudDialog.Cancel();
}
