using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Dashboard;

public partial class MonthlyChart
{
    [Parameter]
    public BranchOption Branch { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ILogger<MonthlyChart> Logger { get; set; } = default!;


    private bool IsLoading = true;
    private List<MonthOption> MonthOptions { get; set; } = new();

    private DateTime SelectedMonth { get; set; }


    private List<ChartSeries> SalesSeries { get; set; } = new();

    private List<ChartSeries> LandedCostSeries { get; set; } = new();

    private List<ChartSeries> ExpenseSeries { get; set; } = new();

    private List<ChartSeries> NetProfitSeries { get; set; } = new();


    private string[] Labels { get; set; } = Array.Empty<string>();


    private ChartOptions SalesOptions = new()
    {
        ChartPalette = new[]
        {
            "#2196F3"
        }
    };


    private ChartOptions LandedCostOptions = new()
    {
        ChartPalette = new[]
        {
            "#FF9800"
        }
    };


    private ChartOptions ExpenseOptions = new()
    {
        ChartPalette = new[]
        {
            "#F44336"
        }
    };


    private ChartOptions NetProfitOptions = new()
    {
        ChartPalette = new[]
        {
            "#4CAF50"
        }
    };


    protected override async Task OnParametersSetAsync()
    {
        CreateMonthOptions();

        SelectedMonth = MonthOptions.First().Date;
        await LoadChart();
    }


    private async Task LoadChart()
    {
        IsLoading = true;

        try
        {
            var data = await Http.GetFromJsonAsync<List<DashboardChartDto>>(
                    $"api/dashboard/monthly-chart/{Branch}?date={SelectedMonth:yyyy-MM-dd}");

            if (data != null)
            {
                Labels = data
                    .Select(x => x.Label)
                    .ToArray();

                CreateSeries(data);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed loading monthly chart");
        }
        finally
        {
            IsLoading = false;
        }
    }


    private void CreateSeries(List<DashboardChartDto> data)
    {
        SalesSeries = CreateChartSeries(
            "Sales",
            data.Select(x => x.NetSales));


        LandedCostSeries = CreateChartSeries(
            "Landed Cost",
            data.Select(x => x.LandedCost));


        ExpenseSeries = CreateChartSeries(
            "Expenses",
            data.Select(x => x.Expenses));


        NetProfitSeries = CreateChartSeries(
            "Net Profit",
            data.Select(x => x.NetProfit));
    }


    private List<ChartSeries> CreateChartSeries(
        string name,
        IEnumerable<decimal> values)
    {
        return new()
        {
            new ChartSeries
            {
                Name = name,
                Data = values
                    .Select(x => (double)x)
                    .ToArray()
            }
        };
    }
    private void CreateMonthOptions()
    {
        var current = PhilippineTime.Now.Date;

        for (int i = 0; i < 6; i++)
        {
            var month = new DateTime(
                current.Year,
                current.Month,
                1)
                .AddMonths(-i);

            MonthOptions.Add(new MonthOption
            {
                Date = month,
                Label = i == 0
                    ? "This Month"
                    : month.ToString("MMMM yyyy")
            });
        }
    }
    private async Task OnMonthChanged(DateTime month)
    {
        SelectedMonth = month;

        await LoadChart();
    }
}