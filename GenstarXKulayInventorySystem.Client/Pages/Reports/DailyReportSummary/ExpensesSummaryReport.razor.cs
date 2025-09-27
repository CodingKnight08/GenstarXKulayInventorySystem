using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class ExpensesSummaryReport
{
    [Parameter] public List<BillingDto> Expenses { get; set; } = new List<BillingDto>();
    [Inject] private ILogger<ExpensesSummaryReport> Logger { get; set; } = default!;

    private bool IsLoading { get; set; } = true;
    private decimal TotalExpenses { get; set; } = 0;

    protected override void OnParametersSet()
    {
        IsLoading = true;
        TotalAMount();
        IsLoading = false;
    }

   
    protected void TotalAMount()
    {
       TotalExpenses = Expenses.Sum(x => x.Amount);
    }
}
