using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Text.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.BranchProduct;

public partial class BranchProductStaff
{
    [Parameter] public int BranchProductId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter]
    protected IMudDialogInstance DialogService { get; set; } = default!;
    [Inject] protected ILogger<BranchProductStaff> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;


    protected List<UserDto> Staffs { get; set; } = new List<UserDto>();
    protected BranchProductDto UpdatedProduct { get; set; } = new BranchProductDto(); 
    protected UserDto SelectedStaff { get; set; } = new UserDto();
    protected bool IsLoading = false;
    protected bool IsSaving = false;

    protected override async Task OnParametersSetAsync()
    {
        await LoadBranchProduct();
        await LoadStaff();

        if (!string.IsNullOrWhiteSpace(UpdatedProduct.UserId)
            && Guid.TryParse(UpdatedProduct.UserId, out var userGuid))
        {
            SelectedStaff = Staffs.FirstOrDefault(s => s.Id == userGuid);
        }
    }

    protected async Task LoadStaff()
    {
        try
        {
            IsLoading = true;
            var response = await HttpClient.GetAsync($"api/user/staffs/{UpdatedProduct.Branch}");
            if (response.IsSuccessStatusCode)
            {
                Staffs = await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
            }
            else
            {
                Logger.LogError($"Failed to load staff. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading staff.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task LoadBranchProduct()
    {
        try
        {
            IsLoading = true;
            var response = await HttpClient.GetAsync($"api/product/{BranchProductId}");
            if (response.IsSuccessStatusCode)
            {
                UpdatedProduct = await response.Content.ReadFromJsonAsync<BranchProductDto>() ?? new BranchProductDto();
            }
            else
            {
                Logger.LogError($"Failed to load branch product. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading branch product.");
        }
        finally
        {
            IsLoading = false;
        }
    }
    protected async Task AssignStaff()
    {
        if (SelectedStaff == null)
            return;

        IsSaving = true;

        try
        {
            var request = new BranchProductDto
            {
                Id = BranchProductId,
                UserId = SelectedStaff.Id.ToString()
            };

            var response = await HttpClient.PutAsJsonAsync(
                "api/user/assign/staff",
                request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

                var message = result?.RootElement
                    .GetProperty("message")
                    .GetString();

                SnackBar.Add(message ?? "Assigned Successfully", Severity.Success);

                DialogService.Close(true);
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<JsonDocument>();

                var message = result?.RootElement
                    .GetProperty("message")
                    .GetString();

                SnackBar.Add(message ?? "Assignment Failed", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            SnackBar.Add("Assigned Failed", Severity.Error);
            Logger.LogError(ex, "Error assigning staff.");
        }
        finally
        {
            IsSaving = false;
        }
    }
    protected void Cancel()
    {
        DialogService.Close(false);
    }
}
