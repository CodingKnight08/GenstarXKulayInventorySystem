using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class GetAllClients
{
    [Parameter, SupplyParameterFromQuery(Name = "pageskip")]
    public int PageSkip { get; set; } = 0;
    [Parameter, SupplyParameterFromQuery(Name = "pagetake")]
    public int PageTake { get; set; } = 10;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<GetAllClients> Logger { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    protected List<ClientDto> Clients { get; set; } = new List<ClientDto>();
    protected BranchOption SelectedBranch { get; set; }
    protected int CurrentPageIndex { get; set; }
    protected bool IsLoading { get; set; } = false;
    protected override void OnInitialized()
    {
        SelectedBranch = UserState.Branch.GetValueOrDefault();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (PageTake <= 0)
            PageTake = 10;

        if (PageSkip < 0)
            PageSkip = 0;




        CurrentPageIndex = PageTake > 0 ? PageSkip / PageTake : 0;

        // Load sales whenever query parameters change
        await LoadClients();
    }
    protected async Task LoadClients()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/client/all/{SelectedBranch}");
            response.EnsureSuccessStatusCode();
            var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>();
            Clients = clients ?? new List<ClientDto>();
        }
        catch (Exception ex) {
            Logger.LogError($"Error loading clients in branch {SelectedBranch}, with error {ex.Message}");
            SnackBar.Add("Failed to load clients", Severity.Error);
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }

    protected async Task OnBranchChange(BranchOption branch)
    {
        SelectedBranch = branch;
        await LoadClients();
        StateHasChanged();
    }

    protected async Task CreateClientAsync()
    {
        var option = new DialogOptions {
            FullWidth=true,
            MaxWidth = MaxWidth.Medium,
            BackdropClick=false,
            CloseButton = true,
            
        };
        var dialog = await DialogService.ShowAsync<CreateClient>("Add Client");

        if(dialog is not null)
        {
            var result = await dialog.Result;
            if(result is not null && !result.Canceled && result.Data is ClientDto)
            {
                await LoadClients();
                StateHasChanged();
            }
        }
    }

    protected void ViewClientAsync(int id)
    {
        NavigationManager.NavigateTo($"/clients/view/{id}?pageskip={PageSkip}&pagetake={PageTake}");
    }

    protected async Task EditClientAsync(int id)
    {
        var parameters = new DialogParameters
        {
            { "ClientId", id }
        };
        var option = new DialogOptions
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Medium,
            BackdropClick = false,
            CloseButton = true,
        };

        var dialog = await DialogService.ShowAsync<UpdateClient>("Edit Client", parameters, option);
        if (dialog is not null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is ClientDto)
            {
                await LoadClients();
                StateHasChanged();
            }
        }
    }

    protected async Task DeleteClientAsync(int id)
    {
        var parameter = new DialogParameters { { "ClientId", id } };
        var option = new DialogOptions
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Small,
            CloseButton = true,
            NoHeader = false,
            BackdropClick = false
        };
        var dialog = await DialogService.ShowAsync<DeleteClient>("", parameter, option);
        if (dialog is not null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is true)
            {
                await LoadClients();
                StateHasChanged();
            }
        }
    }

    private void OnPageChanged(int pageIndex)
    {
        CurrentPageIndex = pageIndex;
        PageSkip = pageIndex * PageTake;
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
            $"/clients?pageskip={PageSkip}&pagetake={PageTake}",
            replace: true
        );
    }
}
