using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class CreatePullOutRequest
{
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    protected PullOutRequestDto NewPullOut { get; set; } = new PullOutRequestDto();

}
