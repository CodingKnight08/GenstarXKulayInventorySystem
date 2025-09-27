using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class NonVoiceSummaryReport
{
    [Parameter] public List<DailySaleDto> NonInvoiceSales { get; set; } = new();
    [Inject] private ILogger<InvoiceSummaryReport> Logger { get; set; } = default!;
    protected decimal TotalAmount { get; set; } = 0;
    protected bool IsLoading { get; set; } = false;

    protected override void OnParametersSet()
    {
        IsLoading = true;
        TotalAMount();
        IsLoading = false;
    }

    

    protected void TotalAMount()
    {
        TotalAmount = NonInvoiceSales.Sum(x => x.TotalAmount ?? 0);
    }
}
