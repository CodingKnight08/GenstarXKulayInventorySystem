using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class GetAllDailySales
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllDailySales> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    private MudTable<DailySaleDto> dailySaleTable;
    protected List<DailySaleDto> Sales { get; set; } = new List<DailySaleDto>();
    protected BranchOption Branch { get; set; } 
    protected bool IsLoading { get; set; } = false;
    protected DateTime Today { get; set; } = DateTime.UtcNow;
    protected DateTime SelectedDate { get; set; } = DateTime.UtcNow;
    protected override void OnInitialized()
    {
        Branch = UserState.Branch.GetValueOrDefault(); 
    }

    protected async Task<TableData<DailySaleDto>> ServerLoadData(TableState state, CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            int skip = state.Page * state.PageSize;
            int take = state.PageSize;
            var response = await HttpClient.GetAsync($"api/sales/paged/by/{Branch}/{SelectedDate:yyyy-MM-dd}?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();
            var paginatedResponse = await response.Content.ReadFromJsonAsync<DailySalePageResultDto<DailySaleDto>>();
            if (paginatedResponse != null)
            {
                return new TableData<DailySaleDto>
                {
                    TotalItems = paginatedResponse.TotalCount,
                    Items = paginatedResponse.Sales
                };
            }
            else
            {
                return new TableData<DailySaleDto> { TotalItems = 0, Items = new List<DailySaleDto>() };
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading paginated sales data: {ex.Message}");
            Snackbar.Add("Failed to load data", Severity.Error);
            return new TableData<DailySaleDto> { TotalItems = 0, Items = new List<DailySaleDto>() };
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }

    protected async Task OnDateChanged(DateTime? newDate)
    {
        if (newDate == null)
            return;

        SelectedDate = newDate.Value;
        await dailySaleTable.ReloadServerData();
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
