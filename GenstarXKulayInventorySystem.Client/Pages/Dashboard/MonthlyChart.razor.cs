using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Dashboard;

public partial class MonthlyChart
{
    [Parameter] public BranchOption Branch { get; set; }

    [Inject]
    HttpClient Http { get; set; } = default!;

    [Inject]
    private ILogger<MonthlyChart> Logger { get; set; } = default!;


    private bool IsLoading = true;

    private bool ShowSales = true;
    private bool ShowLandedCost = true;
    private bool ShowExpenses = false;
    private bool ShowNetProfit = true;


    private List<ChartSeries> Series = new();

    private string[] Labels = Array.Empty<string>();


    private List<ChartSeries> VisibleSeries =>
        Series.Where(x =>
            (x.Name == "Sales" && ShowSales) ||
            (x.Name == "Landed Cost" && ShowLandedCost) ||
            (x.Name == "Expenses" && ShowExpenses) ||
            (x.Name == "Net Profit" && ShowNetProfit)
        ).ToList();



    protected override async Task OnParametersSetAsync()
    {
        await LoadChart();
    }



    private async Task LoadChart()
    {
        IsLoading = true;

        try
        {
            var data = await Http.GetFromJsonAsync<List<DashboardChartDto>>(
                $"api/dashboard/monthly-chart/{Branch}");


            if (data != null)
            {
                Labels = data.Select(x => x.Label).ToArray();

                Series = CreateSeries(data);
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



    private void UpdateChart()
    {
        StateHasChanged();
    }



    private List<ChartSeries> CreateSeries(List<DashboardChartDto> data)
    {
        return new()
        {
            new()
            {
                Name = "Sales",
                Data = data.Select(x => (double)x.NetSales).ToArray()
            },

            new()
            {
                Name = "Landed Cost",
                Data = data.Select(x => (double)x.LandedCost).ToArray()
            },

            new()
            {
                Name = "Expenses",
                Data = data.Select(x => (double)x.Expenses).ToArray()
            },

            new()
            {
                Name = "Net Profit",
                Data = data.Select(x => (double)x.NetProfit).ToArray()
            }
        };
    }
}