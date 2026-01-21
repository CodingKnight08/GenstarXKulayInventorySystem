using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills;

public partial class GetAllWayBills
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    [Inject] private ILogger<GetAllWayBills> Logger { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected List<WayBillDto> WayBills { get; set; } = new List<WayBillDto>();
    private bool IsLoading { get; set; } = false;


    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/waybill/all");
            if (response.IsSuccessStatusCode)
            {
                var waybills = await response.Content.ReadFromJsonAsync<List<WayBillDto>>();
                if(waybills != null)
                {
                    WayBills = waybills;
                }
                else
                {
                    SnackBar.Add("No Waybills found!", Severity.Error);
                }
            }
            else
            {
                SnackBar.Add("Failed to load Waybills", Severity.Warning);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError($"Error loading waybills: {ex.Message}");
            SnackBar.Add("An error occured while loading waybills", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    protected async Task AddWayBill()
    {
        NavigationManager.NavigateTo("/create-waybill");
    }

    protected async Task ViewWayBill(int wayBillId)
    {
        NavigationManager.NavigateTo($"/view-waybill/{wayBillId}");
    }
}
