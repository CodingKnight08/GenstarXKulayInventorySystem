using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Users;

public partial class GetAllRegistrants
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllRegistrants> Logger { get; set; } = default!;

    protected List<RegistrationDto> Applicants = new();
    protected bool IsLoading = true;

    protected async override Task OnInitializedAsync()
    {
        await LoadApplicants();
    }

    private async Task LoadApplicants()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/authentication/all/applicants");
            response.EnsureSuccessStatusCode();
            var applicants = await response.Content.ReadFromJsonAsync<List<RegistrationDto>>();
            Applicants = applicants ?? new List<RegistrationDto>();

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading applicants");
        }
        finally
        {
            IsLoading = false;

        }
    }

    protected async Task ApproveApplicant(int registrantId, string fullName)
    {
        var parameters = new DialogParameters
        {
            { "RegistrantId", registrantId },
            { "FullName", fullName }
        };
        var dialog = await DialogService.ShowAsync<UpdateRegistrant>("Approve Applicant", parameters);
        if (dialog is not null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is true)
            {
                await LoadApplicants();
                StateHasChanged();
            }
        }
    }
}
