using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class AddFine : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int TeamId { get; set; }

    private AddTeamFineRequest _form = null!;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        _form = new AddTeamFineRequest { TeamId = TeamId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(AddTeamFineRouteFactory.Uri, _form);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully added a fine.", Severity.Success);
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }

        _processingForm = false;
        MudDialogInstance.Close();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
