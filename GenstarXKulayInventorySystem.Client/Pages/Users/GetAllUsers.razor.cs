using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Users;

public partial class GetAllUsers
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<GetAllUsers> Logger { get; set; } = default!;
    [Inject] private IDialogService Dialog { get; set; } = default!;
    protected List<UserDto> Users { get; set; } = new();
    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsers();
    }

    protected async Task LoadUsers()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/authentication/all/users");
            response.EnsureSuccessStatusCode();
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            Users = users ?? new List<UserDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error {ex}");
        }
        IsLoading = false;
    }

    protected async Task UpdateAsync(UserDto user)
    {
        var parameters = new DialogParameters
        {
            { "User", user }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, BackdropClick = false };

        var dialog = await Dialog.ShowAsync<UpdateUser>("Update User", parameters, options);
        if (dialog is not null)
        {
            var result = await dialog.Result;
            if (result is not null && !result.Canceled && result.Data is UserDto)
            {
                await LoadUsers();
                StateHasChanged();
            }
        }
    }
}
