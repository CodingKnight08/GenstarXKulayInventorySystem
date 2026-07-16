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
                    DamageItems = WayBillItems.Select(item => new WayBillDamageItemDto
                    {
                        WayBillItemId = item.Id,
                        WayBillItem = item,
                        DamageQuantity = 0,
                        DamageAmount = 0,
                        TotalDamageCost = 0
                    }).ToList();
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


    private void OnDamageQuantityChanged(WayBillDamageItemDto item, decimal quantity)
    {
        item.DamageQuantity = quantity;

        var unitPrice = item.WayBillItem?.ItemPrice ?? 0m;
        item.DamageAmount = unitPrice;
        item.TotalDamageCost = unitPrice * quantity;
        StateHasChanged();
    }

    protected async Task SubmitDamages()
    {
        IsLoading = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync(
                "api/waybill/damage",
                DamageItems);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Damage items saved successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Logger.LogError("Failed to save damage items: {Error}", error);
                SnackBar.Add("Saving damage items failed.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while saving damage waybill items");
            SnackBar.Add("Saving damage items failed.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void Cancel() => MudDialog.Cancel();
}
