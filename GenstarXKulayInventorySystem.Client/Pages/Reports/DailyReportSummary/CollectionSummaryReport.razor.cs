using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class CollectionSummaryReport
{
    [Parameter] public BranchOption Branch { get; set; }
    [Parameter] public DateTime ReportDate { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<CollectionSummaryReport> Logger { get; set; } = default!;

    private List<DailySaleDto> CollectionSales { get; set; } = new();

    private bool IsLoading { get; set; } = true;
    private string ErrorMessage { get; set; } = string.Empty;
    protected decimal TotalAmount { get; set; } = 0;
    protected async override Task OnParametersSetAsync()
    {
        await LoadCollection();
        TotalAMount();
    }

    private async Task LoadCollection()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/sales/all/collection/{ReportDate:yyyy-MM-dd}/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var collectections = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
                if (collectections != null)
                {
                    CollectionSales = collectections;
                }
                else
                {
                    ErrorMessage = "No collection sales data available.";
                    CollectionSales = new List<DailySaleDto>();
                }
            }
            else
            {
                ErrorMessage = $"Failed to load collection sales data: {response.ReasonPhrase}";
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error loading collection summary report for {Branch} on {ReportDate}", Branch, ReportDate);
        }
        finally
        {
            IsLoading = false;
        }
    }
    protected void TotalAMount()
    {
        TotalAmount = CollectionSales.Sum(x => x.TotalAmount ?? 0);
    }
}
