using DynamoLeagueBlazor.Shared.Features.Fines;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Fines;

public partial class Manage : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int FineId { get; set; }
    [Parameter, EditorRequired] public EventCallback OnManageButtonClick { get; set; }

    private ManageFineRequest _form = null!;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        _form = new ManageFineRequest { FineId = FineId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/fines/manage", _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Successfully updated fine.", Severity.Success);
                await OnManageButtonClick.InvokeAsync();
            }
            else
            {
                SnackBar.Add("Something went wrong...", Severity.Error);
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        _processingForm = false;
        MudDialogInstance.Close();
    }

    private void IsApproved(bool approved)
    {
        _form.Approved = approved;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
