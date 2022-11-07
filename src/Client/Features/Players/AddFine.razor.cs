using DynamoLeagueBlazor.Shared.Features.Players;

namespace DynamoLeagueBlazor.Client.Features.Players;

public sealed partial class AddFine : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [CascadingParameter] MudDialogInstance MudDialogInstance { get; set; } = null!;
    [Parameter, EditorRequired] public int PlayerId { get; set; }

    private AddFineRequest _form = null!;
    private FineDetailResult? _fineDetail;
    private bool _processingForm;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _form = new AddFineRequest { PlayerId = PlayerId };

        await GetPlayerFineDetailsAsync();
    }

    private async Task GetPlayerFineDetailsAsync()
    {
        _fineDetail = await HttpClient.GetFromJsonAsync<FineDetailResult>(FineDetailRouteFactory.Create(PlayerId), _cts.Token);
    }

    private async Task OnValidSubmitAsync()
    {
        _processingForm = true;

        var response = await HttpClient.PostAsJsonAsync(AddFineRouteFactory.Uri, _form, _cts.Token);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully added a fine. An administrator will either approve or deny the fine.", Severity.Success);
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
