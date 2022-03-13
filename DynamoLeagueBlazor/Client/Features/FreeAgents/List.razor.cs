using DynamoLeagueBlazor.Shared.Features.FreeAgents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DynamoLeagueBlazor.Client.Features.FreeAgents;

public sealed partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private FreeAgentListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;
    private const string _title = "Free Agents";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;
            _result = await HttpClient.GetFromJsonAsync<FreeAgentListResult>("api/freeagents", _cts.Token) ?? new();
            _loading = false;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private bool FilterFunc(FreeAgentListResult.FreeAgentItem item)
    {
        if (string.IsNullOrWhiteSpace(_searchValue))
            return true;
        if ($"{item.Name} {item.Position} {item.Team} {item.BiddingEnds} {item.HighestBid}".Contains(_searchValue))
            return true;
        return false;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
