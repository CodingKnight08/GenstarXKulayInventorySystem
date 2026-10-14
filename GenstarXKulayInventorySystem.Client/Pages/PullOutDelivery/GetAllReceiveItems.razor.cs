using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class GetAllReceiveItems
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<GetAllReceiveItems> Logger { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    protected List<PullOutRequestDto> RecievePullOuts { get; set; } = new List<PullOutRequestDto>();
    protected bool IsLoading { get; set; } = false;
    private BranchOption Branch { get; set; }


    protected override async Task OnInitializedAsync()
    {
        Branch = UserState.Branch.GetValueOrDefault();
        await LoadData();
    }

    protected async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/pullout/all/recieved/{Branch}");
            if (response.IsSuccessStatusCode) { 
            
               var pullouts = await response.Content.ReadFromJsonAsync<List<PullOutRequestDto>>();
                if(pullouts is not null)
                {
                    RecievePullOuts = pullouts.ToList();
                }
            }
            else
            {
                SnackBar.Add("Failed to load data", Severity.Error);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error occured");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ViewRecieve(int id)
    {
        NavigationManager.NavigateTo($"/view-recieve/{id}");
    }
}
