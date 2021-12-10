using DynamoLeagueBlazor.Shared.Features.Teams;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
using static DynamoLeagueBlazor.Shared.Features.Teams.GetTeamDetailResult;

namespace DynamoLeagueBlazor.Client.Features.Teams;

public partial class Detail
{
    [Inject] private HttpClient HttpClient { get; set; } = null!;

    [Parameter] public int TeamId { get; set; }

    private GetTeamDetailResult? _result;
    private string _playerTableHeader = "Rostered Players";
    private IEnumerable<PlayerItem> _playersToDisplay = Array.Empty<PlayerItem>();
    private string _title = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _result = await HttpClient.GetFromJsonAsync<GetTeamDetailResult>($"teams/{TeamId}");
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

    private void ShowDroppedPlayers()
    {
        _playersToDisplay = _result!.DroppedPlayers;
        _playerTableHeader = "Dropped Players";
    }
}
