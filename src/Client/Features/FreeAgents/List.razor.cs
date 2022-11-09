using DynamoLeagueBlazor.Shared.Features.FreeAgents;

namespace DynamoLeagueBlazor.Client.Features.FreeAgents;

public sealed partial class List : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }

    private FreeAgentListResult _result = new();
    private bool _loading;
    private string _searchValue = string.Empty;
    private const string _title = "Free Agents";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        _result = await HttpClient.GetFromJsonAsync<FreeAgentListResult>(FreeAgentListRouteFactory.Uri, _cts.Token) ?? new();
        _loading = false;
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
