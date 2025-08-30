using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports;

public partial class GetAllDailyReport
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<GetAllDailyReport> Logger { get; set; } = default!;
    [Inject] protected IDialogService Dialog { get; set; } = default!;
    protected List<DailySaleReportDto> DailySaleReports { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected BranchOption SelectedBranch { get; set; } = BranchOption.GeneralSantosCity;
    protected override async Task OnInitializedAsync()
    {
        await LoadDailySaleReports();
    }

    protected async Task LoadDailySaleReports()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/dailysalereport/all/{SelectedBranch}");
            if (response.IsSuccessStatusCode)
            {
                var reports = await response.Content.ReadFromJsonAsync<List<DailySaleReportDto>>();
                if (reports != null)
                {
                    DailySaleReports = reports;
                }
            }
            else
            {
                Logger.LogError("Failed to load daily sale reports. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, "Error loading daily sale reports");
        }
        finally
        {
            await Task.Delay(2000);
            IsLoading = false;
        }
    }
    private async Task OnBranchChanged(BranchOption newBranch)
    {
        SelectedBranch = newBranch;
        await LoadDailySaleReports();
    }

    protected async Task CreateReport()
    {
       var parameters = new DialogParameters { ["Branch"] = SelectedBranch };
       var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, BackdropClick = false };
        var dialog = await Dialog.ShowAsync<CreateDailySaleReport>("", parameters,options);
       if(dialog != null)
       {
           var result = await dialog.Result;
           if (result is not null && !result.Canceled)
           {
               await LoadDailySaleReports();
               StateHasChanged();
           }
        }
    }
}
