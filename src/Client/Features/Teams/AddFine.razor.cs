using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class AddFine : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar SnackBar { get; set; }
    [CascadingParameter] public required MudDialogInstance MudDialogInstance { get; set; }
    [Parameter, EditorRequired] public required int TeamId { get; set; }

    private AddFineRequest _form = null!;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override void OnInitialized()
    {
        _form = new AddFineRequest { TeamId = TeamId };
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(AddFineRouteFactory.Uri, _form, _cts.Token);

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
