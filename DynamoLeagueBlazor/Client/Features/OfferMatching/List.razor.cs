using System.Net.Http.Json;
using DynamoLeagueBlazor.Shared.Features.OfferMatching;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DynamoLeagueBlazor.Client.Features.OfferMatching;

public sealed partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private const string _title = "Offer Matching";
    private bool _loading;
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

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
