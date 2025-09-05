using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

namespace GenstarXKulayInventorySystem.Client;

public class AuthorizationMessageHandler:DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigation;

    public AuthorizationMessageHandler(ILocalStorageService localStorage, NavigationManager navigation)
    {
        _localStorage = localStorage;
        _navigation = navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Optional: if token is invalid/expired, redirect to login
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _localStorage.RemoveItemAsync("authToken");
            _navigation.NavigateTo("/login", forceLoad: true);
        }

        return response;
    }
}
