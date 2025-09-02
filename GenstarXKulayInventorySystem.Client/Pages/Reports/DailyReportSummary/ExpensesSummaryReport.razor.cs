using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports.DailyReportSummary;

public partial class ExpensesSummaryReport
{
    [Parameter] public DateTime ReportDate { get; set; }
    [Parameter] public BranchOption Branch { get; set; }

    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<ExpensesSummaryReport> Logger { get; set; } = default!;

    private bool IsLoading { get; set; } = true;
    private List<BillingDto> Expenses { get; set; } = new();
    private decimal TotalExpenses { get; set; } = 0;

    protected async override Task OnParametersSetAsync()
    {
        await LoadExpenses();
        TotalAMount();
    }

    protected async Task LoadExpenses()
    {
        try
        {
            BillingBranch branch = Branch switch
            {
                BranchOption.GeneralSantosCity => BillingBranch.GenStar,
                BranchOption.Polomolok => BillingBranch.Kulay,
                BranchOption.Warehouse => BillingBranch.Warehouse,
                _ => BillingBranch.GenStar
            };

            var response = await HttpClient.GetAsync($"api/billings/all/expenses/{ReportDate:yyyy-MM-dd}/{branch}");
            if (response.IsSuccessStatusCode)
            {
                var expenses = await response.Content.ReadFromJsonAsync<List<BillingDto>>()
                               ?? new List<BillingDto>();

                Expenses = expenses.OrderBy(e => e.Category).ToList();
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading expenses.");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    protected void TotalAMount()
    {
       TotalExpenses = Expenses.Sum(x => x.Amount);
    }
}
