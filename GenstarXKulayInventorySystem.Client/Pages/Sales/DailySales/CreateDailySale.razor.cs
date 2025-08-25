using GenstarXKulayInventorySystem.Client.Pages.Clients;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class CreateDailySale
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<CreateDailySale> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogInstance { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected DailySaleDto Sale { get; set; } = new DailySaleDto();
    protected List<ClientDto> Clients { get; set; } = new List<ClientDto>();
    protected ClientDto NewClient { get; set; } = new ClientDto();
    protected string Client { get; set; } = string.Empty;
    protected bool IsLoading { get; set; } = false;
    protected bool IsClientLoading { get; set; } = false;
    protected string? ErrorMessage { get; set; }
    protected bool IsValid => !string.IsNullOrWhiteSpace(Sale.NameOfClient) && !string.IsNullOrWhiteSpace(Sale.RecieptReference) && Sale.SalesOption != null && Sale.SaleItems.Count != 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadClients();
    }

    protected async Task LoadClients()
    {
        IsClientLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/client/all/{Sale.Branch}");
            response.EnsureSuccessStatusCode();
            var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>();
            Clients = clients ?? new List<ClientDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading clients data: {ex.Message}");
            Snackbar.Add("Failed to load clients data", Severity.Error);
        }
        finally
        {
            IsClientLoading = false;
        }
    }
    protected async Task CreateSaleAsync()
    {
        IsLoading = true;
        try
        {
            if (Sale.ClientId == null && !string.IsNullOrWhiteSpace(Client))
            {
                // Show confirmation dialog
                var dialog = await DialogService.ShowAsync<ConfirmAddClient>(
                    "",
                    new DialogParameters { { "ClientName", Client } }
                );

                var result = await dialog.Result;

                if (!result.Canceled)
                {
                    NewClient.ClientName = Client;
                    NewClient.Branch = Sale.Branch;

                    var clientResponse = await HttpClient.PostAsJsonAsync("api/client", NewClient);
                    clientResponse.EnsureSuccessStatusCode();

                    Snackbar.Add("New client added successfully!", Severity.Success);

                    var createdClientId = await clientResponse.Content.ReadFromJsonAsync<int?>();
                    if (createdClientId.HasValue)
                    {
                        Sale.ClientId = createdClientId.Value;
                        Sale.NameOfClient = Client;
                    }

                }
            }

            var response = await HttpClient.PostAsJsonAsync("api/sales", Sale);
            response.EnsureSuccessStatusCode();

            Snackbar.Add("Sale created successfully!", Severity.Success);
            NavigationManager.NavigateTo("/sales");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to create sale with error, {ex.Message}";
            Logger.LogError(ex, "Error occurred creating sale");
        }
        finally
        {
            IsLoading = false;
        }
    }


    protected Task<IEnumerable<string>> SearchClients(string value, CancellationToken cancellationToken)
    {
        if(Clients is null || !Clients.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        // if text is null or empty, show complete list
        var result = Clients.Where(c => !string.IsNullOrWhiteSpace(c.ClientName) && (string.IsNullOrWhiteSpace(value) || c.ClientName.Contains(value, StringComparison.OrdinalIgnoreCase)))
                           .Select(c => c.ClientName!); 
        return Task.FromResult(result);
    }

    protected void OnClientNameTyped(string clientName)
    {
        Sale.NameOfClient = clientName;
        Client = clientName;
        var matchedClient = Clients.FirstOrDefault(c => c.ClientName.Equals(clientName, StringComparison.OrdinalIgnoreCase));
        if (matchedClient != null)
        {
            Sale.ClientId = matchedClient.Id;
        }
    }
    private async Task OnBranchChanged(BranchOption newBranch)
    {
        Sale.Branch = newBranch;

        // Reset client selection
        Sale.ClientId = null;
        Sale.NameOfClient = string.Empty;
        Client = string.Empty;

        // Reload clients for the new branch
        await LoadClients();
    }
    protected void Cancel()
    {
        NavigationManager.NavigateTo("/sales");
    }
    private void HandleSaleItemsChanged(List<SaleItemDto> saleItems)
    {
        Sale.SaleItems = saleItems; // 🔗 Bind sale items to DailySaleDto
    }

 

}
