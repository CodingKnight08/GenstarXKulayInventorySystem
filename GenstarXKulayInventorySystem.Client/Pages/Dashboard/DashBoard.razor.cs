using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Dashboard;

public partial class DashBoard
{
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<DashBoard> Logger { get; set; } = default!;
    [Inject] private UserState User { get; set; } = default!;

    protected DashBoardDto DashBoardData { get; set; } = new DashBoardDto();
    protected DateTime SelectedDate { get; set; } = DateTime.UtcNow.Date;
    private bool IsLoading { get; set; } = true;
    private bool IsDailyLoading = true;
  

    protected override async Task OnInitializedAsync()
    {
       
        await Task.Delay(1000);
        await ReloadDashboard();
    }


    protected async Task LoadNetAndCogs()
    {
        IsDailyLoading = true;

        try
        {
            var branch = User.Branch;

            var response = await Http.GetFromJsonAsync<DashBoardDto>(
                $"api/dashboard/daily/{branch}?date={SelectedDate:yyyy-MM-dd}");

            if (response != null)
            {
                DashBoardData = response;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load dashboard sales data.");
        }
        finally
        {
            IsDailyLoading = false;
        }
    }
    protected async Task LoadExpenses()
    {
        IsDailyLoading = true;

        try
        {
            var branch = User.Branch;

            var utcDate = DateTime.SpecifyKind(SelectedDate, DateTimeKind.Utc);

            var response = await Http.GetFromJsonAsync<decimal>(
                $"api/dashboard/daily-expenses/{branch}?date={utcDate:O}");
            DashBoardData.Expenses = response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load daily expenses.");
        }
        finally
        {
            IsDailyLoading = false;
        }
    }

    private async Task OnDateChanged(DateTime? date)
    {
        if (date.HasValue)
        {
            SelectedDate = date.Value.Date;
            await ReloadDashboard();
        }
    }
    private async Task ReloadDashboard()
    {
        await Task.WhenAll(
            LoadNetAndCogs(),
            LoadExpenses()
        );
    }

}
