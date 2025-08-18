using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class GetAllDailySales
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllDailySales> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    protected List<DailySaleDto> Sales { get; set; } = new List<DailySaleDto>();
    protected bool IsLoading { get; set; } = false;
    protected DateTime Today { get; set; } = UtilitiesHelper.GetPhilippineTime();
    protected DateTime SelectedDate { get; set; } = UtilitiesHelper.GetPhilippineTime();
    protected override async Task OnInitializedAsync()
    {
        await LoadDailySales();
    }

    protected async Task LoadDailySales()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/sales/all");
            response.EnsureSuccessStatusCode();
            var sales = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
            Sales = sales ?? new List<DailySaleDto>();
        }
        catch (Exception ex) {

            Logger.LogError($"Error loading sales data: {ex.Message}");
            Snackbar.Add("Failed to load data", Severity.Error);
        }
        finally
        {
            await Task.Delay(2000);
            IsLoading = false;
        }
    }


    protected async Task OnDateChanged(DateTime? newDate)
    {
        if (newDate == null)
            return;

        SelectedDate = newDate.Value;
        await LoadSalesByDate(SelectedDate);
    }

    protected async Task LoadSalesByDate(DateTime date)
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/sales/by-date/{date:yyyy-MM-dd}");
            if (response.IsSuccessStatusCode)
            {
                Sales = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>() ?? new();
            }
            else
            {
                Sales.Clear();
                Snackbar.Add($"No sales found for {date:MMMM dd, yyyy}.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading sales for {date}: {ex.Message}");
            Snackbar.Add("Failed to load data", Severity.Error);
        }
        finally
        {
            await Task.Delay(2000);
            IsLoading = false;
        }
    }
    protected void CreateSaleAsync()
    {
        NavigationManager.NavigateTo("/sales/create");
    }

    protected void ViewSaleAsync(int Id)
    {
        NavigationManager.NavigateTo($"sales/view/{Id}");
    }
}
