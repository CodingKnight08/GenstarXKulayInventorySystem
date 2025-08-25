using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class GetAllClients
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<GetAllClients> Logger { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!; 
    protected List<ClientDto> Clients { get; set; } = new List<ClientDto>();
    protected BranchOption SelectedBranch { get; set; } = BranchOption.GeneralSantosCity;

    protected bool IsLoading { get; set; } = false;
    protected override async Task OnInitializedAsync()
    {
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
}
