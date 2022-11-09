using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class List : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }

    private TeamListResult? _result;
    private const string _title = "Teams";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _result = await HttpClient.GetFromJsonAsync<TeamListResult>(TeamListRouteFactory.Uri, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
