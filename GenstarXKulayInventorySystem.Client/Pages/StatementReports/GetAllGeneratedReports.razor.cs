using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.StatementReports;

public partial class GetAllGeneratedReports
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<GetAllGeneratedReports> Logger { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;

    protected List<ClientDto> Clients { get; set; } = new List<ClientDto>();
    protected bool IsLoading { get; set; } = true;
    protected BranchOption Branch { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Branch = UserState?.Branch ?? BranchOption.GeneralSantosCity;
        await LoadData();
    }

    protected async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/statementreport/clients/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>();
                if (clients != null)
                {
                    Clients = clients;
                }
            }
            else
            {
                Logger.LogError("Failed to load clients. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading clients");
        }
        finally
        {
            IsLoading = false;
        }
    }


    protected void ViewClientChargeSales(int ClientId)
    {
        NavigationManager.NavigateTo($"view/statement/{ClientId}");
    }
}
