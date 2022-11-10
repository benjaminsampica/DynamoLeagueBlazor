using DynamoLeagueBlazor.Client.Shared.Components;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;

namespace DynamoLeagueBlazor.Client.Features.OfferMatching;

public sealed partial class List : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }
    [Inject] public required ISnackbar SnackBar { get; set; }
    [Inject] public required IConfirmDialogService ConfirmDialogService { get; set; }

    private const string _title = "Offer Matching";
    private bool _loading;
    private readonly CancellationTokenSource _cts = new();
    private OfferMatchingListResult _result = new();

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        await LoadDataAsync();
        _loading = false;
    }
    private async Task LoadDataAsync()
    {
        _result = await HttpClient.GetFromJsonAsync<OfferMatchingListResult>(OfferMatchingListRouteFactory.Uri, _cts.Token) ?? new();
    }

    private async Task MatchPlayerAsync(int playerId)
    {
        if (await ConfirmDialogService.IsCancelledAsync()) return;

        var response = await HttpClient.PostAsJsonAsync(MatchPlayerRouteFactory.Uri, new MatchPlayerRequest(playerId));

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully matched player.", Severity.Success);
            await LoadDataAsync();
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }
    }
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
