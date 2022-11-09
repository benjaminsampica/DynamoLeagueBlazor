using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class SignPlayer : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [CascadingParameter] public required MudDialogInstance MudDialogInstance { get; set; }
    [Parameter, EditorRequired] public required int PlayerId { get; set; }

    private SignPlayerRequest _form = null!;
    private SignPlayerDetailResult? _signPlayerDetailResult;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _form = new SignPlayerRequest { PlayerId = PlayerId };

        await SignPlayerDetailsAsync();
    }

    private async Task SignPlayerDetailsAsync()
    {
        _signPlayerDetailResult = await HttpClient.GetFromJsonAsync<SignPlayerDetailResult>(SignPlayerRouteFactory.Create(PlayerId), _cts.Token);
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(SignPlayerRouteFactory.Uri, _form);

        if (response.IsSuccessStatusCode)
        {
            Snackbar.Add("Successfully signed player.", Severity.Success);
        }
        else
        {
            Snackbar.Add("Something went wrong...", Severity.Error);
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
