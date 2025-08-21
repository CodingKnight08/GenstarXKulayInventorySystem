using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class DeleteDailySale
{
    [Parameter] public DailySaleDto Sale { get; set; } = new();
    [CascadingParameter] public IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<DeleteDailySale> Logger { get; set; } = default!;
    protected bool IsLoading { get; set; } = false;

    protected async Task DeleteAsync()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/sales/{Sale.Id}");
            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Daily Sale deleted!",Severity.Success);
                Dialog.Close(DialogResult.Ok(true));
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
        
    }
    protected void Cancel() => Dialog.Cancel();
}
