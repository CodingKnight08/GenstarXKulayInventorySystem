using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills.WayBillDamageItems;

public partial class AddWayBillDamageItems
{
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public int WayBillId { get; set; }

    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected ILogger<AddWayBillDamageItems> Logger { get; set; } = default!;
    protected List<WayBillItemsDto> WayBillItems { get; set; } = new List<WayBillItemsDto>();
    protected List<WayBillDamageItemDto> DamageItems { get; set; } = new List<WayBillDamageItemDto>();
    private bool IsLoading { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        await LoadItems();
    }

    protected async Task LoadItems()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/waybill/items/{WayBillId}");
            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<List<WayBillItemsDto>>();
                if (items != null)
                {
                    WayBillItems = items;
                }
                else
                {
                    SnackBar.Add("No Items Found!", Severity.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

}
