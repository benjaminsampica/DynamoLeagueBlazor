using DynamoLeagueBlazor.Shared.Features.Fines;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.Players;

public partial class AddFine
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int PlayerId { get; set; }

    private AddFineRequest _form = null!;
    private bool _processingForm;

    protected override async Task OnInitializedAsync()
    {
        if (PlayerId < 1) throw new ArgumentOutOfRangeException();

        _form = new AddFineRequest { PlayerId = PlayerId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("fines/add", _form);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Successfully added fine.", Severity.Success);
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

    private void Cancel() => MudDialogInstance.Close();
}
