using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class ChargeSaleSummaryReport
{
    [Parameter] public BranchOption Branch { get; set; }
    [Parameter] public DateTime ReportDate { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<InvoiceSummaryReport> Logger { get; set; } = default!;
    protected List<DailySaleDto> ChargeSales { get; set; } = new();
    protected decimal TotalAmount { get; set; } = 0;
    protected bool IsLoading { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadInvoice();
        TotalAMount();
    }

    protected async Task LoadInvoice()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/sales/all/unpaid/{ReportDate:yyyy-MM-dd}/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var invoices = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
                if (invoices != null || invoices.Count > 0)
                {
                    ChargeSales = invoices;
                }
            }
            else
            {
                Logger.LogError($"Failed to load invoice summary report: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, "Error loading invoice summary report");
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }

    protected void TotalAMount()
    {
        TotalAmount = ChargeSales.Sum(x => x.TotalAmount ?? 0);
    }
}
