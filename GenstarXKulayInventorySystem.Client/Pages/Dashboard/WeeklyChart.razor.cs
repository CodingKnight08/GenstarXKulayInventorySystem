using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Dashboard;

public partial class WeeklyChart
{
    [Parameter]
    public BranchOption Branch { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ILogger<WeeklyChart> Logger { get; set; } = default!;


    private List<WeekOptionDto> WeekOptions { get; set; } = new();

    private DateTime SelectedWeek { get; set; }

    private bool IsLoading { get; set; }

    private List<ChartSeries> Series { get; set; } = new();

    private string[] Labels { get; set; } = Array.Empty<string>();

    private List<ExpensesDto> WeeklyExpenses { get; set; } = new();


    protected override async Task OnInitializedAsync()
    {
        CreateWeekOptions();

        if (WeekOptions.Any())
        {
            SelectedWeek = WeekOptions.First().StartDate;
        }

        await LoadData();
    }


    protected override async Task OnParametersSetAsync()
    {
        if (Series.Count == 0 && WeekOptions.Any())
        {
            await LoadData();
        }
    }


    private async Task LoadData()
    {
        try
        {
            IsLoading = true;


           await LoadChart();
           await LoadWeeklyExpenses();
            
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task LoadChart()
    {
        try
        {
            var result = await Http.GetFromJsonAsync<List<DashboardChartDto>>(
                $"api/dashboard/weekly-chart/{Branch}?date={SelectedWeek:yyyy-MM-dd}"
            );


            if (result == null)
            {
                Labels = Array.Empty<string>();
                Series.Clear();
                return;
            }


            Labels = result
                .Select(x => x.Label)
                .ToArray();


            Series = CreateSeries(result);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed loading weekly chart");
        }
    }


    private async Task LoadWeeklyExpenses()
    {
        try
        {
            var result = await Http.GetFromJsonAsync<List<ExpensesDto>>(
                $"api/dashboard/weekly-expenses/{Branch}?date={SelectedWeek:yyyy-MM-dd}"
            );


            WeeklyExpenses = result ?? new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed loading weekly expenses");
        }
    }


    private List<ChartSeries> CreateSeries(
        List<DashboardChartDto> data)
    {
        return new()
        {
            new ChartSeries
            {
                Name = "Sales",
                Data = data
                    .Select(x => (double)x.NetSales)
                    .ToArray()
            },

            new ChartSeries
            {
                Name = "Landed Cost",
                Data = data
                    .Select(x => (double)x.LandedCost)
                    .ToArray()
            },

            new ChartSeries
            {
                Name = "Expenses",
                Data = data
                    .Select(x => (double)x.Expenses)
                    .ToArray()
            },

            new ChartSeries
            {
                Name = "Net Profit",
                Data = data
                    .Select(x => (double)x.NetProfit)
                    .ToArray()
            }
        };
    }


    private void CreateWeekOptions()
    {
        WeekOptions.Clear();

        var today = PhilippineTime.Now.Date;


        var currentWeekStart = today.AddDays(
            today.DayOfWeek == DayOfWeek.Sunday
                ? -6
                : DayOfWeek.Monday - today.DayOfWeek
        );


        for (int i = 0; i < 4; i++)
        {
            var start = currentWeekStart.AddDays(-7 * i);
            var end = start.AddDays(6);


            WeekOptions.Add(new WeekOptionDto
            {
                StartDate = start,
                Label = i == 0
                    ? "This Week"
                    : $"{start:dd MMM} - {end:dd MMM}"
            });
        }
    }


    private async Task OnWeekChanged(DateTime date)
    {
        if (SelectedWeek == date)
            return;


        SelectedWeek = date;

        await LoadData();
    }
}