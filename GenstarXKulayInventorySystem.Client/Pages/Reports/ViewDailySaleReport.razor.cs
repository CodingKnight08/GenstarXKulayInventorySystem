using GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports;

public partial class ViewDailySaleReport
{
    [Parameter] public int ReportId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewDailySaleReport> Logger { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager Navigation { get; set; } = default!;

    protected DailySaleReportDto DailySaleReport { get; set; } = new();
    protected List<DailySaleDto> PaidSales { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected override async Task OnInitializedAsync()
    {
        await LoadReport();
    }

    protected async Task LoadReport()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/dailysalereport/{ReportId}");
            if (response.IsSuccessStatusCode)
            {
                DailySaleReport = await response.Content.ReadFromJsonAsync<DailySaleReportDto>() ?? new DailySaleReportDto();
            }
            else
            {
                Logger.LogError("Failed to load daily sale report with ID {ReportId}. Status Code: {StatusCode}", ReportId, response.StatusCode);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error loading daily sale report with ID {ReportId}", ReportId);
        }
        finally
        {
           IsLoading = false;
        }
    }
    protected List<BreadcrumbItem> _items =
      [
          new("Daily Sale Reports", href: "/reports"),
            new("View Daily Sale Report", href: null, disabled: true)
      ];

    private string GetBranchDisplayName(BranchOption branch)
    {
        return branch switch
        {
            BranchOption.GeneralSantosCity => "Genstar Paint Trade Center",
            BranchOption.Polomolok => "Kulay Paint Supply",
            BranchOption.Warehouse => "Warehouse",
            _ => branch.ToString()
        };
    }

    protected async Task DeleteReport() {
        try
        {
            var parameter = new DialogParameters
            {
                {"DailySaleReportId", ReportId }
            };
            var options = new DialogOptions 
            { CloseButton = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                BackdropClick = false 
            };
            var dialog = await DialogService.ShowAsync<DeleteDailySaleReport>("", parameter, options);

            // Wait for the dialog result
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                // Refresh list after delete
                Navigation.NavigateTo("/reports");
            }

        }
        catch(Exception ex)
        {
            Logger.LogError($"Error occured: {ex.Message}");
        }
    
    }
}
