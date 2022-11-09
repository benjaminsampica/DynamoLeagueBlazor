using DynamoLeagueBlazor.Shared.Features.Dashboard;

namespace DynamoLeagueBlazor.Client.Features.Dashboard;

public sealed partial class TopTeamFines : IDisposable
{
    [Inject] public required HttpClient HttpClient { get; set; }

    private TopTeamFinesResult? _result;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        _result = await HttpClient.GetFromJsonAsync<TopTeamFinesResult>(TopTeamFinesRouteFactory.Uri, _cts.Token);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}