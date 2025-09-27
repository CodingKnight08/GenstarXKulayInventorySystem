using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class InvoiceSummaryReport
{
    [Parameter] public List<DailySaleDto> InvoicesSales { get; set; } = new();
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<InvoiceSummaryReport> Logger { get; set; } = default!;
    protected decimal TotalAmount { get; set; } = 0;
    protected bool IsLoading { get; set; } = false;

    protected override void OnParametersSet()
    {
        TotalAMount();
    }

    
    protected void TotalAMount()
    {
        TotalAmount = InvoicesSales.Sum(x => x.TotalAmount ?? 0);
    }
}
