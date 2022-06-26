using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using MudBlazor;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.OfferMatching;

public sealed partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Inject] private ISnackbar SnackBar { get; set; } = null!;
    private const string _title = "Offer Matching";
    private bool _loading;
    private MatchPlayerRequest _player;
    private readonly CancellationTokenSource _cts = new();
    private OfferMatchingListResult _result = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<OfferMatchingListResult>(OfferMatchingListRouteFactory.Uri, _cts.Token) ?? new();
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
    private async void MatchPlayerAsync(int playerId, int amount)
    {
        _player = new MatchPlayerRequest() { PlayerId = playerId, Amount = amount };
        var response = await HttpClient.PostAsJsonAsync(OfferMatchingListRouteFactory.Uri, _player);

        if (response.IsSuccessStatusCode)
        {
            SnackBar.Add("Successfully retained player.", Severity.Success);
        }
        else
        {
            SnackBar.Add("Something went wrong...", Severity.Error);
        }
        await OnInitializedAsync();
    }
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
