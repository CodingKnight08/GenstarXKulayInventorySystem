using DocumentFormat.OpenXml.Wordprocessing;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery.PullOuts;

public partial class GetAllPullOuts
{
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<GetAllPullOuts> Logger { get; set; } = default!;
    protected List<PullOutRequestDto> PullOuts { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    private BranchOption Branch { get; set; }
    protected override async Task OnInitializedAsync()
    {
        Branch = UserState.Branch ?? BranchOption.Warehouse;
        await LoadPullOutRequests();

    }

    protected async Task LoadPullOutRequests()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/pullout/all/requester/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var pullOuts = await response.Content.ReadFromJsonAsync<List<PullOutRequestDto>>();
                if (pullOuts != null)
                {
                    PullOuts = pullOuts;
                }
            }
            else
            {
                Logger.LogError("Failed to load pull-out requests. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading pull-out requests.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ViewPullOut(int id)
    {
        NavigationManager.NavigateTo($"/pullout/detail/{id}");
    }
}
