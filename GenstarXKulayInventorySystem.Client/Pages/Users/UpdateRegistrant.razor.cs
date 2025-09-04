using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Users;

public partial class UpdateRegistrant
{
    [Parameter] public int RegistrantId { get; set; }
    [Parameter] public string FullName { get; set; } = string.Empty;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;

    protected async Task RegisterApplicant()
    {
        var response = await HttpClient.PutAsync( $"api/authentication/approve-applicant/{RegistrantId}",null);

        if (response.IsSuccessStatusCode)
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
        else
        {
            MudDialog.Close(DialogResult.Cancel());
        }
    }

    protected void Cancel() => MudDialog.Cancel();
}
