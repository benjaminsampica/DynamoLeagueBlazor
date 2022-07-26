using DynamoLeagueBlazor.Client.Shared.Components;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;

namespace DynamoLeagueBlazor.Client.Features.OfferMatching;

public sealed partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    [Inject] private IConfirmDialogService ConfirmDialogService { get; set; } = null!;

    private const string _title = "Offer Matching";
    private bool _loading;
    private readonly CancellationTokenSource _cts = new();
    private OfferMatchingListResult _result = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            await LoadDataAsync();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        finally
        {
            _loading = false;
        }
    }
    private async Task LoadDataAsync()
    {
        _result = await HttpClient.GetFromJsonAsync<OfferMatchingListResult>(OfferMatchingListRouteFactory.Uri, _cts.Token) ?? new();
    }

    private async Task MatchPlayerAsync(int playerId)
    {
        if (await ConfirmDialogService.IsCancelledAsync()) return;

        var response = await HttpClient.PostAsJsonAsync(OfferMatchingListRouteFactory.Uri, new MatchPlayerRequest() { PlayerId = playerId });

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully retained player.", Severity.Success);
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
