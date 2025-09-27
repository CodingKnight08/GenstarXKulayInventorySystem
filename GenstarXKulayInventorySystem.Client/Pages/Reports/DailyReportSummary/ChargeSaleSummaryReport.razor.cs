using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class ChargeSaleSummaryReport
{
    [Parameter] public List<DailySaleDto> ChargeSales { get; set; } = new List<DailySaleDto>();
    [Inject] private ILogger<InvoiceSummaryReport> Logger { get; set; } = default!;
    protected decimal TotalAmount { get; set; } = 0;
    protected bool IsLoading { get; set; } = false;

    protected override void OnParametersSet()
    {
        TotalAMount();
    }


    protected void TotalAMount()
    {
        TotalAmount = ChargeSales.Sum(x => x.TotalAmount ?? 0);
    }
}
