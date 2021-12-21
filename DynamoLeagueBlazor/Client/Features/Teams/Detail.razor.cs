using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
using static DynamoLeagueBlazor.Shared.Features.Teams.TeamDetailResult;

namespace DynamoLeagueBlazor.Client.Features.Teams;

[Authorize]
public partial class Detail : IDisposable
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;
    [Parameter] public int TeamId { get; set; }

    private TeamDetailResult? _result;
    private string _playerTableHeader = "Rostered Players";
    private IEnumerable<PlayerItem> _playersToDisplay = Array.Empty<PlayerItem>();
    private string _title = string.Empty;
    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<TeamDetailResult>($"teams/{TeamId}", _cts.Token);
            _title = $"Team Detail - {_result!.TeamName}";
            ShowRosteredPlayers();
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    private void ShowRosteredPlayers()
    {
        _playersToDisplay = _result!.RosteredPlayers;
        _playerTableHeader = "Rostered Players";
    }

    private void ShowUnrosteredPlayers()
    {
        _playersToDisplay = _result!.UnrosteredPlayers;
        _playerTableHeader = "Unrostered Players";
    }

    private void ShowUnsignedPlayers()
    {
        _playersToDisplay = _result!.UnsignedPlayers;
        _playerTableHeader = "Unsigned Players";
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
