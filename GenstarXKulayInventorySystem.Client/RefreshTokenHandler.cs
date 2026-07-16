using Blazored.LocalStorage;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client;

public class RefreshTokenHandler:DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly IHttpClientFactory _httpClientFactory;
    public RefreshTokenHandler(
    ILocalStorageService localStorage,
    NavigationManager navigationManager,
    IHttpClientFactory httpClientFactory)
    {
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Don't intercept login/refresh requests
        if (request.RequestUri!.AbsolutePath.Contains("/login") ||
            request.RequestUri.AbsolutePath.Contains("/refresh"))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var accessToken = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await Logout();
            return response;
        }

        var refreshResponse = await RefreshAccessToken(accessToken!, refreshToken);

        if (refreshResponse == null)
        {
            await Logout();
            return response;
        }

        // Retry original request
        var clonedRequest = await CloneRequestAsync(request);

        clonedRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshResponse.AccessToken);

        return await base.SendAsync(clonedRequest, cancellationToken);
    }

    private async Task<LoginResponseDto?> RefreshAccessToken(
        string accessToken,
        string refreshToken)
    {
        var client = new HttpClient
        {
            BaseAddress = InnerHandler is HttpClientHandler
                ? null
                : null
        };

        var response = await client.PostAsJsonAsync(
            "https://localhost:5001/api/authentication/refresh",
            new RefreshTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        if (result == null)
            return null;

        await _localStorage.SetItemAsync("authToken", result.AccessToken);
        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

        return result;
    }

    private async Task Logout()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");

        _navigationManager.NavigateTo("/login", true);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;

            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
