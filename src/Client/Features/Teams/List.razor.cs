using DynamoLeagueBlazor.Shared.Features.Teams;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public sealed partial class List : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    private TeamListResult? _result;
    private const string _title = "Teams";
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TeamListResult>(TeamListRouteFactory.Uri, _cts.Token);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
