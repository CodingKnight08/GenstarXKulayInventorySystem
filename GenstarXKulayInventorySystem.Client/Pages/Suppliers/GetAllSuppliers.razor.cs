using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Suppliers;

public partial class GetAllSuppliers
{
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected ILogger<GetAllSuppliers> Logger { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected List<SupplierDto> Suppliers { get; set; } = new();
    protected bool IsLoading { get; set; } = true;


    protected override async Task OnInitializedAsync()
    {
        await LoadSuppliers();
    }

    protected async Task LoadSuppliers()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync("api/supplier/all");
            response.EnsureSuccessStatusCode();
            var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
            Suppliers = suppliers ?? new List<SupplierDto>();

        }
        catch(Exception ex)
        {
            Logger.LogError($"Error {ex}");
        }
        IsLoading = false;
    }

    protected async Task AddSupplier()
    {
        var dialogRef = await DialogService.ShowAsync<CreateSupplier>("Create Supplier Info");
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled && result.Data is SupplierDto)
            {
                await LoadSuppliers();
                StateHasChanged();
            }
        }
    }

    protected async Task EditSupplier(int supplierId)
    {
        var parameter = new DialogParameters
        {
            {"SupplierId",supplierId }
        };
        var dialogRef = await DialogService.ShowAsync<EditSupplier>("Edit Supplier Info", parameter);
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                await LoadSuppliers();
                StateHasChanged();
            }
        }
    }

    protected async Task DeleteSupplier(int supplierId)
    {
        var parameter = new DialogParameters
        {
            {"SupplierId", supplierId }
        };
        var dialogRef = await DialogService.ShowAsync<DeleteSupplier>("", parameter);
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                await LoadSuppliers();
                StateHasChanged();
            }
        }
    }


}
