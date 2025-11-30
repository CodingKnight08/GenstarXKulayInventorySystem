using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class GetAllRequestProducts
{
    [Parameter, SupplyParameterFromQuery(Name = "pageskip")]
    public int PageSkip { get; set; } = 0;
    [Parameter, SupplyParameterFromQuery(Name = "pagetake")]
    public int PageTake { get; set; } = 10;
    [Inject] public UserState UserState { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllRequestProducts> Logger { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected bool IsLoading { get; set; } = false;
    protected List<PullOutRequestDto> PullOuts { get; set; } = new List<PullOutRequestDto>();
    protected BranchOption Branch { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        Branch = UserState.Branch ?? BranchOption.GeneralSantosCity;
        await LoadData();
    }
    protected override void OnParametersSet()
    {
         if(PageTake <= 0)
            PageTake = 10;

        if (PageSkip < 0)
            PageSkip = 0;

    }
    protected async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/pullout/all/requests/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                PullOuts = await response.Content.ReadFromJsonAsync<List<PullOutRequestDto>>() ?? new List<PullOutRequestDto>();
                if(PullOuts.Count == 0)
                {
                    SnackBar.Add("No Pull Outs", Severity.Warning);
                }
                
            }
            else
            {
                SnackBar.Add("Failed to load pullouts", Severity.Error);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error loading pullouts");
            SnackBar.Add("Failed to load pull outs", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task RequestPullOut()
    {
        try
        {
            var options = new DialogOptions
            {
                CloseButton = true,
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
                BackdropClick = false
            };

            var dialog = await DialogService.ShowAsync<CreatePullOutRequest>(
                "Create Pull-Out Request",
                options
            );

            var result = await dialog.Result;

            if (!result.Canceled)
            {
                // You can get data from dialog: result.Data
                SnackBar.Add("Pull-out request created successfully!", Severity.Success);

                // Optional: refresh table or reload list
                await LoadData();
                StateHasChanged();
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error in creating pull out request");
            SnackBar.Add("An error occured.", Severity.Error);
        }
    }

    protected void ViewPullOutDetails(int pullOutId)
    {
        NavigationManager.NavigateTo($"/view-pull-out-request/{pullOutId}?pageskip={PageSkip}&pagetake={PageTake}");
    }
    private void OnPageChanged(int page)
    {
        PageSkip = page;
        UpdateQuery();
    }

    private void OnRowsPerPageChanged(int size)
    {
        PageTake = size;
        PageSkip = 0;
        UpdateQuery();
    }

    private void UpdateQuery()
    {
        NavigationManager.NavigateTo(
            $"/requestitems?pageskip={PageSkip}&pagetake={PageTake}",
            replace: true
        );
    }
}
