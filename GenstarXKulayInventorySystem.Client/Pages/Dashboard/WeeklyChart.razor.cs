using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Dashboard;

public partial class WeeklyChart
{
    [Parameter] public BranchOption Branch { get; set; }
    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private ILogger<WeeklyChart> Logger { get; set; } = default!;


    private bool IsLoading = true;

    private List<ChartSeries> Series { get; set; } = new();

    private string[] Labels { get; set; } = Array.Empty<string>();


    protected override async Task OnParametersSetAsync()
    {
        await LoadChart();
    }


    private async Task LoadChart()
    {
        try
        {
            var result = await Http.GetFromJsonAsync<List<DashboardChartDto>>(
                $"api/dashboard/weekly-chart/{Branch}");


            if (result != null)
            {
                Labels = result
                    .Select(x => x.Label)
                    .ToArray();


                Series = CreateSeries(result);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed loading weekly chart");
        }
        finally
        {
            IsLoading = false;
        }
    }


    private List<ChartSeries> CreateSeries(List<DashboardChartDto> data)
    {
        return new()
        {
            new ChartSeries
            {
                Name="Sales",
                Data=data.Select(x=>(double)x.NetSales).ToArray()
            },

            new ChartSeries
            {
                Name="Landed Cost",
                Data=data.Select(x=>(double)x.LandedCost).ToArray()
            },

            new ChartSeries
            {
                Name="Expenses",
                Data=data.Select(x=>(double)x.Expenses).ToArray()
            },

            new ChartSeries
            {
                Name="Net Profit",
                Data=data.Select(x=>(double)x.NetProfit).ToArray()
            }
        };
    }
}
