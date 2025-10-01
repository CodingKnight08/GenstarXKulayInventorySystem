using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.OperationalProvider;

public partial class GetAllOperationalProvider
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<GetAllOperationalProvider> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    protected List<OperationsProviderDto> OperationsProviders { get; set; } = new();
    private MudTable<OperationsProviderDto>? operationsProviderTable;
    protected bool IsLoading { get; set; } = false;
    protected string? ErrorMessage { get; set; }

    protected async Task<TableData<OperationsProviderDto>> ServerLoadData(TableState state, CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            int skip = state.Page * state.PageSize;
            int take = state.PageSize;
            var response = await HttpClient.GetAsync($"api/operationsprovider/paged/by/{UserState.Branch.GetValueOrDefault()}?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();
            var paginatedResponse = await response.Content.ReadFromJsonAsync<OperationsProviderPageResultDto<OperationsProviderDto>>();
            if (paginatedResponse != null)
            {
                return new TableData<OperationsProviderDto>
                {
                    TotalItems = paginatedResponse.TotalCount,
                    Items = paginatedResponse.OperationsProviders
                };
            }
            else
            {
                return new TableData<OperationsProviderDto> { TotalItems = 0, Items = new List<OperationsProviderDto>() };
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading paginated operations providers data: {ex.Message}");
            Snackbar.Add("Failed to load data", Severity.Error);
            return new TableData<OperationsProviderDto> { TotalItems = 0, Items = new List<OperationsProviderDto>() };
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }

    protected async Task CreateOperationalProvider()
    {
        var dialog = await DialogService.ShowAsync<AddOperationalProvider>("Add Provider",
            new DialogParameters
            {
                ["Branch"] = UserState.Branch.GetValueOrDefault()
            }
        );
        if(dialog is not null)
        {
            var result = await dialog.Result;
            if(result is not null && !result.Canceled && result.Data is OperationsProviderDto)
            {
                await operationsProviderTable!.ReloadServerData();
                StateHasChanged();
            }
        }
    }

    protected void ViewProvider(int id)
    {
        NavigationManager.NavigateTo($"operational-providers/view/{id}");
    }

    protected async Task UpdateOperationalProvider(int id)
    {
        var dialog = await DialogService.ShowAsync<UpdateOperationalProvider>("Update Provider",
            new DialogParameters
            {
                ["Id"] = id,
            }
        );
        if (dialog is not null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is int)
            {
                await operationsProviderTable!.ReloadServerData();
                StateHasChanged();
            }
        }
    }




}
